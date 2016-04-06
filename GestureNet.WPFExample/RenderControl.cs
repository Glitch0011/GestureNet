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
                        new PathGeometry(new List<PathFigure> {pathFigure}));

                    drawingContext.DrawEllipse(Brushes.Black, myPen, points.Last(), 5, 5);
                }
            }

            base.OnRender(drawingContext);
        }
    }
}