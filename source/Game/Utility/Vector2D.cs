﻿using System;

namespace Game.Utility
{
    /// <summary>
    /// Represents a 2 dimensional Vector usable for positioning and stuff.
    /// </summary>
    public struct Vector2D
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2D(float x, float y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        public Vector2D(Vector2D other)
            : this()
        {
            this.X = other.X;
            this.Y = other.Y;
        }

        #region Object

        public override bool Equals(object obj)
        {
            if (obj is Vector2D) {
                Vector2D v = (Vector2D) obj;

                if (v.X == X && v.Y == Y) {
                    return obj.GetType().Equals(this.GetType());
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{{X={0}, Y={1}}}", X, Y);
        }

        #endregion

        public float Normalize()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public static bool operator ==(Vector2D lh, Vector2D rh)
        {
            if (lh.X == rh.X && lh.Y == rh.Y) {
                return true;
            }
            else {
                return false;
            }
        }

        public static bool operator !=(Vector2D lh, Vector2D rh)
        {
            return lh != rh;
        }

        public static Vector2D operator +(Vector2D lh, Vector2D rh)
        {
            return new Vector2D(lh.X + rh.X, lh.Y + rh.Y);
        }

        public static Vector2D operator -(Vector2D lh, Vector2D rh)
        {
            return new Vector2D(lh.X - rh.X, lh.Y - rh.Y);
        }

        public static Vector2D operator *(Vector2D lh, float rh)
        {
            return new Vector2D(rh * lh.X, rh * lh.Y);
        }

        public static Vector2D operator /(Vector2D lh, float rh)
        {
            return new Vector2D(lh.X / rh, lh.Y / rh);
        }

        public static Vector2D operator -(Vector2D rh)
        {
            return new Vector2D(-rh.X, -rh.Y);
        }

    }
}
