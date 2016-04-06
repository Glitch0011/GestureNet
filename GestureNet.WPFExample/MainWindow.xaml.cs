using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GestureNet.IO;
using GestureNet.Recognisers;
using GestureNet.Structures;
using Timer = System.Timers.Timer;

namespace GestureNet.WPFExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer RecordingTimer { get; } = new Timer
        {
            Interval = 10,
        };

        private Timer RecognitionTimer { get; } = new Timer
        {
            Interval = 100
        };

        private List<TimedPoint> Points { get; } = new List<TimedPoint>();

        private List<Gesture> TrainingSet { get; set; } // training set loaded from XML files

        public MainWindow()
        {
            InitializeComponent();

            RecordingTimer.Elapsed += RecordingTimer_Elapsed;
            RecognitionTimer.Elapsed += DetectionTimer_Elapsed;
            
            try
            {
                TrainingSet = GestureLoader.ReadGestures(new FileInfo("gestures.json")).ToList();
                RenderControl.Points = () => this.SmoothPoints;
            }
            catch (Exception)
            {
                Debugger.Break();
            }
        }

        private float SmoothDistance { get; } = 10.0f;
        private float Smoothness { get; } = 0.1f;

        private IEnumerable<System.Windows.Point> SmoothPoints
        {
            get
            {
                return
                    CatmullRom.Smooth(Points.Select(x => new Vector2((float)x.Point.X, (float)x.Point.Y)).ToList(), SmoothDistance,
                        Smoothness)
                        .Select(p => new System.Windows.Point(p.X, p.Y)).ToList();
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

                Dispatcher.Invoke(() =>
                {
                    ResultsView.ItemsSource = results;
                });
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

    public class ValueConverter : IMultiValueConverter
    {
        public string Threshhold { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = values.FirstOrDefault() as Result;

            float threshold;
            var textBox = values.Skip(1).FirstOrDefault() as TextBox;

            if (textBox != null && float.TryParse(textBox.Text, out threshold))
                return result != null && result.Score < threshold;
            else
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public struct TimedPoint
    {
        public DateTime Creation { get; set; }
        public Vector2 Point { get; set; }
    }

    public class RenderControl : UIElement
    {
        public Func<IEnumerable<System.Windows.Point>> Points { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var myPen = new Pen(Brushes.Black, 10);
            var myBluePen = new Pen(Brushes.LightBlue, 10);

            drawingContext.DrawRectangle(Brushes.LightBlue, myBluePen, new Rect(0, 0, RenderSize.Width, RenderSize.Height));

            if (Points != null)
            {
                var points = Points().ToList();

                if (points.Count > 2)
                {
                    var pathFigure = new PathFigure
                    {
                        IsClosed = false,
                        IsFilled = false,
                        StartPoint = points[0],
                        Segments = new PathSegmentCollection(),
                    };

                    for (var i = 1; i < points.Count; i++)
                    {
                        pathFigure.Segments.Add(new LineSegment()
                        {
                            IsSmoothJoin = true,
                            Point = points[i],
                            IsStroked =true
                        });
                    }
                    var geometry = new PathGeometry(new List<PathFigure>() {pathFigure});
                    drawingContext.DrawGeometry(Brushes.Black, myPen, geometry);
                }
            }
            
            base.OnRender(drawingContext);
        }
    }
}
