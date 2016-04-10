using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GestureNet.IO;
using GestureNet.Recognisers;
using GestureNet.Structures;
using Timer = System.Timers.Timer;

namespace GestureNet.WPFExample
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer RecordingTimer { get; } = new Timer
        {
            Interval = 10
        };

        private Timer RecognitionTimer { get; } = new Timer
        {
            Interval = 200
        };

        private List<TimedPoint> Points { get; } = new List<TimedPoint>();

        private List<Gesture> TrainingSet { get; }

        /// <summary>
        /// Minimum distance between points when smoothed
        /// </summary>
        private float SmoothDistance { get; } = 5.0f;

        /// <summary>
        /// Smoothness of the CatmullRom filter
        /// </summary>
        private float Smoothness { get; } = 0.1f;

        /// <summary>
        /// Runs the points though a CatmullRom filter and then translate then into Windows.Points
        /// </summary>
        private IEnumerable<Point> SmoothPoints
        {
            get { return SmoothGesturePoints.Select(p => new Point(p.X, p.Y)).ToList(); }
        }

        /// <summary>
        /// Run the points through a CatmullRom filter
        /// </summary>
        private IList<Vector2> SmoothGesturePoints
        {
            get
            {
                return
                    CatmullRom.Smooth(Points.Select(x => new Vector2(x.Point.X, x.Point.Y)).ToList(), SmoothDistance,
                        Smoothness).ToList();
            }
        }

        /// <summary>
        /// Main Window constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            RecordingTimer.Elapsed += RecordingTimer_Elapsed;
            RecognitionTimer.Elapsed += DetectionTimer_Elapsed;

            try
            {
				TrainingSet = GestureLoader.ReadGestures(new FileInfo("gestures.xml")).ToList();
			}
            catch (Exception)
            {
				TrainingSet = new List<Gesture>();
            }

			RenderControl.Points = () => SmoothPoints;
		}

        /// <summary>
        /// Detect weither we are performing live-recognition, and if so, asyncronously to the UI thread, try recognise the current points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetectionTimer_Elapsed(object sender, EventArgs e)
        {
            var process = false;

            Dispatcher.Invoke(() =>
            {
                if (chkLiveRecognition.IsChecked.HasValue && chkLiveRecognition.IsChecked.Value)
                    process = true;
            });

            if (process)
                UpdateResults();
        }

        /// <summary>
        /// Locate where the mouse currently is, and add it to a list of current points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecordingTimer_Elapsed(object sender, EventArgs e)
        {
            //Protect against two timer events overlapping
            if (Monitor.TryEnter(Points, TimeSpan.FromMilliseconds(5)))
            {
                try
                {
                    var mousePos = MouseUtilities.GetMousePosition();

                    Dispatcher.Invoke(() =>
                    {
                        mousePos = RenderControl.PointFromScreen(mousePos);

                        if (!MouseUtilities.IsMouseButtonDown(MouseButton.Left) &&
                            !MouseUtilities.IsMouseButtonDown(MouseButton.Right))
                        {
                            StopRecording();
                            return;
                        }

                        Points.Add(new TimedPoint
                        {
                            Point = new Vector2((float) mousePos.X, (float) mousePos.Y),
                            Creation = DateTime.Now
                        });

                        long mi;
                        if (long.TryParse(txtNumeric.Text, out mi))
                            Points.RemoveAll(x => (DateTime.Now - x.Creation).TotalMilliseconds > mi);

                        var s = this.SmoothPoints;

                        RenderControl.InvalidateVisual();
                    });
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
                finally
                {
                    Monitor.Exit(Points);
                }
            }
        }

        /// <summary>
        /// Classify the current points and update the dynamic-list on the UI
        /// </summary>
        private void UpdateResults()
        {
            try
            {
                var results = PointCloudRecognizer.Classify(new Gesture(SmoothGesturePoints),
                    TrainingSet);

                Dispatcher.Invoke(() => { ResultsView.ItemsSource = results; });
            }
            catch (ArgumentException)
            {
            }
            catch (Exception e)
            {
                //Somtimes we'll hit a collection-modified exception
                //Handling it like this is actually cheaper than locking it with the remove-all statement
            }
        }

        /// <summary>
        /// Initiate the recording of points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GestureCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StartRecording();
        }

        /// <summary>
        /// Initiate the recording and classification of points
        /// </summary>
        private void StartRecording()
        {
            RecordingTimer.Start();
            RecognitionTimer.Start();
        }

        /// <summary>
        /// Stop the recording and classification of points
        /// </summary>
        private void StopRecording()
        {
            RecordingTimer.Stop();
            RecognitionTimer.Stop();
            Points.Clear();
            RenderControl.InvalidateVisual();
        }

        /// <summary>
        /// When the mouse is released, either classify the points, or stop them as a new gesture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GestureCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        UpdateResults();
                        break;
                    case MouseButton.Right:
                        if (SmoothGesturePoints.Count > 0)
                            TrainingSet.Add(new Gesture(SmoothGesturePoints, txtName.Text));
                        break;
                    default:
                        break;
                }

                StopRecording();
            }
            catch (Exception)
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// When the window closes safely, save the gestures to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            GestureLoader.SaveGestures(new FileInfo("gestures.xml"), TrainingSet);
        }
    }
}