using System;
using System.Numerics;

namespace GestureNet.WPFExample
{
    public struct TimedPoint
    {
        public DateTime Creation { get; set; }
        public Vector2 Point { get; set; }
    }
}