using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GestureNet.Recognisers;
using GestureNet.Structures;
using Point = GestureNet.Structures.Point;
using GestureNet.IO;

namespace GestureNet.Example
{
    public partial class ExampleForm : Form
    {
        public ExampleForm()
        {
            InitializeComponent();
        }

        private readonly Timer mouseRecorder = new Timer()
        {
            Interval = 10
        };

        private readonly List<Point> points = new List<Point>();

        private readonly Timer paintRender = new Timer()
        {
            Interval = 10,
        };

        private BufferedGraphics Graphics { get; set; }

        private List<Gesture> trainingSet = new List<Gesture>(); // training set loaded from XML files

        private void Form1_Load(object sender, EventArgs e)
        {
            trainingSet = GestureLoader.ReadGestures(new FileInfo("data.bin")).ToList();

            MouseDown += (o, args) =>
            {
                mouseRecorder.Start();
            };

            MouseUp += (o, args) =>
            {
                mouseRecorder.Stop();

                if (args.Button == MouseButtons.Left)
                {
					try
					{
						var guess = PointCloudRecognizer.Classify(new Gesture(points), trainingSet);

						label1.Text = string.Join(Environment.NewLine, guess.Select(x => x));
					}
					catch (ArgumentException)
					{
						return;
					}
					finally 
					{
						points.Clear();
					}
                }
                else if (args.Button == MouseButtons.Right)
                {
                    trainingSet.Add(new Gesture(points, textBox1.Text));
                }
            };

            mouseRecorder.Tick += (o, args) =>
            {
                var formPos = PointToClient(MousePosition);

                points.Add(new GestureNet.Structures.Point(formPos.X, formPos.Y, 0));
            };

            Graphics = BufferedGraphicsManager.Current.Allocate(this.CreateGraphics(), this.DisplayRectangle);

            paintRender.Tick += (o, args) =>
            {
                var gra = Graphics.Graphics;
                gra.Clear(Color.CornflowerBlue);

                gra.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gra.SmoothingMode = SmoothingMode.HighQuality;

                RenderPoints(points.Select(x => new PointF(x.X, x.Y)).ToList(), gra);

                foreach (var template in this.trainingSet)
                {
                    RenderPoints(template.Points.Select(x => new PointF(x.X, x.Y)).ToList(), gra);
                }

                Graphics.Render();
            };

            paintRender.Start();

            this.Closing += (o, args) =>
            {
                GestureLoader.SaveGestures(new FileInfo("data.bin"), trainingSet);
            };
        }

        private static void RenderPoints(IReadOnlyList<PointF> renderPoints, Graphics gra)
        {
            var path = new GraphicsPath(FillMode.Winding);

            for (var i = 0; i < renderPoints.Count - 1; i++)
            {
                var a = renderPoints[i];
                var b = renderPoints[i + 1];

                path.AddLine(new System.Drawing.PointF(a.X, a.Y), new PointF(b.X, b.Y));
            }

            gra.DrawPath(Pens.Black, path);
        }

        /// <summary>
        /// Loads training gesture samples from XML files
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Gesture> LoadTrainingSet()
        {
            return
                new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.xml", SearchOption.AllDirectories)
                    .SelectMany(IO.GestureLoader.ReadGestures)
                    .Where(x => x != null);
        }
    }
}
