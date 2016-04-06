using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
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
            Interval = 0
        };

        private List<TimedPoint> Points { get; } = new List<TimedPoint>();

        private List<Gesture> TrainingSet { get; }

        private float SmoothDistance { get; } = 10.0f;
        private float Smoothness { get; } = 0.1f;

        private IEnumerable<Point> SmoothPoints
        {
            get
            {
                return
                    CatmullRom.Smooth(Points.Select(x => new Vector2(x.Point.X, x.Point.Y)).ToList(), SmoothDistance,
                        Smoothness)
                        .Select(p => new Point(p.X, p.Y)).ToList();
            }
        }

        private IReadOnlyList<Vector2> SmoothGesturePoints
        {
            get
            {
                return
                    CatmullRom.Smooth(Points.Select(x => new Vector2(x.Point.X, x.Point.Y)).ToList(), SmoothDistance,
                        Smoothness).ToList();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            RecordingTimer.Elapsed += RecordingTimer_Elapsed;
            RecognitionTimer.Elapsed += DetectionTimer_Elapsed;

            try
            {
                TrainingSet = GestureLoader.ReadGestures(new FileInfo("gestures.json")).ToList();
                RenderControl.Points = () => SmoothPoints;
            }
            catch (Exception)
            {
                Debugger.Break();
            }
        }

        private void DetectionTimer_Elapsed(object sender, EventArgs e)
        {
            var process = false;

            Dispatcher.Invoke(() =>
            {
                if (chkLiveRecognition.IsChecked.HasValue && chkLiveRecognition.IsChecked.Value)
                    process = true;
            });

            if (process)
            {
                UpdateResults();
            }
        }

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

        private void GestureCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StartRecording();
        }

        private void StartRecording()
        {
            RecordingTimer.Start();
            RecognitionTimer.Start();
        }

        private void StopRecording()
        {
            RecordingTimer.Stop();
            RecognitionTimer.Stop();
            Points.Clear();
        }

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

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            GestureLoader.SaveGestures(new FileInfo("gestures.json"), TrainingSet);
        }
    }
}