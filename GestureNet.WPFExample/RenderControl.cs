using GestureNet.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GestureNet.WPFExample
{
    public class RenderControl : UIElement
    {
        public Func<IEnumerable<Point>> Points { get; set; }
		public Func<IEnumerable<Gesture>> Gestures { get; set; }
		public Func<string> SelectedGesture { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var myPen = new Pen(Brushes.Black, 10);
            var myBluePen = new Pen(Brushes.LightBlue, 10);

            drawingContext.DrawRectangle(Brushes.LightBlue, myBluePen,
                new Rect(0, 0, RenderSize.Width, RenderSize.Height));

            if (Points != null)
            {
                var points = Points().ToList();

                if (points.Count > 2)
				{
					RenderPoints(drawingContext, myPen, points);
				}
			}

			if (SelectedGesture() != null)
			{
				var selectedGestures = Gestures().Where(x => x.Name == SelectedGesture());

				var redPen = new Pen(Brushes.Red, 8);

				foreach (var gesture in selectedGestures)
				{
					RenderPoints(drawingContext, redPen, gesture.Points.Select(x =>
					{
						var width = RenderSize.Width - 100;
						var width_offset = 70;

						var pos = new Point(width_offset + (width / 2) + (width * x.x), (RenderSize.Height / 2) + (RenderSize.Height * x.y));
						return new Point(pos.X, pos.Y);
					}).ToList());
				}
			}

            base.OnRender(drawingContext);
        }

		private static void RenderPoints(DrawingContext drawingContext, Pen myPen, List<Point> points)
		{
			var pathFigure = new PathFigure
			{
				IsClosed = false,
				IsFilled = false,
				StartPoint = points[0],
				Segments = new PathSegmentCollection()
			};

			for (var i = 1; i < points.Count; i++)
			{
				pathFigure.Segments.Add(new LineSegment
				{
					IsSmoothJoin = true,
					Point = points[i],
					IsStroked = true
				});
			}

			drawingContext.DrawGeometry(Brushes.Black, myPen,
				new PathGeometry(new List<PathFigure> { pathFigure }));

			drawingContext.DrawEllipse(Brushes.Black, myPen, points.Last(), 5, 5);
		}
	}
}