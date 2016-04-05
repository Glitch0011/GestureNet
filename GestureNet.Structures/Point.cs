namespace GestureNet.Structures
{
    /// <summary>
    ///     Implements a 2D Point that exposes X, Y, and StrokeID properties.
    ///     StrokeID is the stroke index the point belongs to (e.g., 0, 1, 2, ...) that is filled by counting pen down/up
    ///     events.
    /// </summary>
    public class Point : IPoint
    {
        public float X { get; }
        public float Y { get; }
        public int StrokeId { get; }

        public Point(float x, float y, int strokeId)
        {
            X = x;
            Y = y;
            StrokeId = strokeId;
        }

        public override string ToString()
        {
            return X + ":" + Y;
        }
    }
}