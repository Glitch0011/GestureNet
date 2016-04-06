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
using System.Windows.Media;
using System.Windows.Shapes;
using GestureNet.IO;
using GestureNet.Recognisers;
using GestureNet.Structures;
using Point = GestureNet.Structures.Point;
using Timer = System.Timers.Timer;

namespace GestureNet.WPFExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private struct TimedPoint
        {
            public DateTime Creation { get; set; }
            public Point Point { get; set; }
        }

        private Timer RecordingTimer { get; }
        private List<TimedPoint> Points { get; }

        private List<Gesture> TrainingSet { get; set; } // training set loaded from XML files
        
        public MainWindow()
        {
            InitializeComponent();
            
            RecordingTimer = new Timer()
            {
                Interval = 10
            };

            RecordingTimer.Elapsed += RecordingTimer_Elapsed;

            Points = new List<TimedPoint>();

            TrainingSet = GestureLoader.ReadGestures(new FileInfo("gestures.json")).ToList();
        }

        private IEnumerable<System.Windows.Point> SmoothPoints
        {
            get
            {
                return CatmullRom.Smooth(Points.Select(x => new Vector2(x.Point.X, x.Point.Y)).ToList(), 10.0f, 0.1f)
                    .Select(p => new System.Windows.Point(p.X, p.Y)).ToList();
            }
        }

        private IReadOnlyList<Point> SmoothGesturePoints
        {
            get
            {
                return CatmullRom.Smooth(Points.Select(x => new Vector2(x.Point.X, x.Point.Y)).ToList(), 10.0f, 0.1f)
                    .Select(p => new Point(p.X, p.Y, 0)).ToList();
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
                        mousePos = GestureCanvas.PointFromScreen(mousePos);

                        if (Mouse.LeftButton == MouseButtonState.Released &&
                            Mouse.RightButton == MouseButtonState.Released)
                            RecordingTimer.Stop();

                        Points.Add(new TimedPoint()
                        {
                            Point = new Point((float) mousePos.X, (float) mousePos.Y, 0),
                            Creation = DateTime.Now
                        });

                        long mi;
                        if (long.TryParse(txtNumeric.Text, out mi))
                        {
                            Points.RemoveAll(x => (DateTime.Now - x.Creation).TotalMilliseconds > mi);
                        }

                        if (Points.Count > 0)
                        {

                            var canvas = GestureCanvas;

                            canvas.Children.Clear();

                            var collection = new PointCollection();

                            foreach (var p in SmoothPoints)
                                collection.Add(p);

                            var line = new Polyline
                            {
                                Points = collection,
                                Stroke = new SolidColorBrush(Colors.Black),
                                StrokeThickness = 3
                            };

                            canvas.Children.Add(line);

                        }

                        if (chkLiveRecognition.IsChecked.HasValue && chkLiveRecognition.IsChecked.Value)
                            UpdateResults();
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
                ResultsView.ItemsSource = PointCloudRecognizer.Classify(new Gesture(SmoothGesturePoints),
                    TrainingSet);
            }
            catch (ArgumentException)
            {
                return;
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }

        private void GestureCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordingTimer.Start();
        }

        private void GestureCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RecordingTimer.Stop();

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

                GestureCanvas.Children.Clear();
                Points.Clear();
            }
            catch (Exception ex)
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
