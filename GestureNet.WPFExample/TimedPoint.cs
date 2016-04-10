using GestureNet.Structures;
using System;

namespace GestureNet.WPFExample
{
    public struct TimedPoint
    {
        public DateTime Creation { get; set; }
        public Vector2 Point { get; set; }
    }
}