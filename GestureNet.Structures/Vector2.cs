using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureNet.Structures
{
	public class Vector2
	{
		public float x;
		public float y;

		public float X
		{
			get
			{
				return x;
			}
			set
			{
				x = value;
			}
		}

		public float Y
		{
			get
			{
				return y;
			}
			set
			{
				y = value;
			}
		}

		public Vector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2()
		{
			this.x = 0;
			this.y = 0;
		}

		public static float Distance(Vector2 a, Vector2 b)
		{
			return (a - b).Magnitude;
		}

		public static float DistanceSquared(Vector2 a, Vector2 b)
		{
			return (a - b).SqrMagnitude;
		}

		public float Magnitude
		{
			get
			{
				return (float)Math.Sqrt(SqrMagnitude);
			}
		}
		public float SqrMagnitude
		{
			get
			{
				return ((x * x) + (y * y));
			}
		}

		public override string ToString()
		{
			return x + ":" + y;
		}

		public static Vector2 operator +(Vector2 v1, Vector2 v2)
		{
			return new Vector2(
			   v1.X + v2.X,
			   v1.Y + v2.Y);
		}

		public static Vector2 operator -(Vector2 v1, Vector2 v2)
		{
			return new Vector2(
			   v1.X - v2.X,
			   v1.Y - v2.Y);
		}
	}
}
