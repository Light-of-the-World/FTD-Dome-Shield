using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvShields
{
    public class Elipse
    {
        // Radius
        public float Width { get; set; }
        public float Height { get; set; }
        public float Length { get; set; }

        public Quaternion Rotation { get; set; }
        public Vector3 Position { get; set; }

        private AdvShieldProjector _controller;

        public Elipse(AdvShieldProjector item)
        {
            _controller = item;
            UpdateInfo();
        }

        public void UpdateInfo()
        {
            AdvShieldData d = _controller.ShieldData;

            Width = d.Width / 2;
            Height = d.Height / 2;
            Length = d.Length / 2;

            //probably changing the shield not the block
            //_controller.GameWorldPosition;
            Position = _controller.ShieldDome.transform.position;
            Rotation = _controller.GameWorldRotation;
        }

        public bool CheckIntersection(Vector3 rayPosition, Vector3 rayDirection, out Vector3 hitPosition, out Vector3 hitNormal)
        {
            hitPosition = Vector3.zero;
            hitNormal = Vector3.forward;

            // Solution from https://forums.ogre3d.org/viewtopic.php?t=26442
            var rp = AdjustPoint(rayPosition);
            var rd = AdjustDirection(rayDirection);

            var x2 = Width * Width;
            var y2 = Height * Height;
            var z2 = Length * Length;

            var a = rd.x * rd.x / x2 + rd.y * rd.y / y2 + rd.z * rd.z / z2;
            var b = 2 * rp.x * rd.x / x2 + 2 * rp.y * rd.y / y2 + 2 * rp.z * rd.z / z2;
            var c = rp.x * rp.x / x2 + rp.y * rp.y / y2 + rp.z * rp.z / z2 - 1;

            var d = b * b - 4 * a * c;

            if (d < 0)
                return false;

            d = (float)Math.Sqrt(d);

            var h1 = (-b + d) / (2 * a);
            var h2 = (-b - d) / (2 * a);

            if (h1 < 0 || h2 < 0)
            {
                hitPosition = Vector3.zero;
                hitNormal = Vector3.forward;
                return false;
            }

            var range = h1 < h2 ? h1 : h2;

            hitPosition = rayPosition + rayDirection.normalized * range;
            hitNormal = (hitPosition - Position).normalized;

            return true;
        }

        public bool CheckIntersection(Vector3 position, float radius)
        {
            //Debug.Log("Advanced Shields: Check if inside shield.");

            if (IsInside(position))
                return false;

            //Debug.Log("Advanced Shields: Is not inside shield.");

            // Avoid creating a new object
            var w = Width;
            var h = Height;
            var l = Length;

            Width += radius;
            Height += radius;
            Length += radius;

            var intersects = IsInside(position);

            // Avoid float point errors by storing and reassigning values
            Width = w;
            Height = h;
            Length = l;

            return intersects;
        }

        public float SurfaceArea()
        {
            var p = 1.6075;

            var list = new List<float>(3) { Width, Height, Length };
            list.Sort();

            var a = list[2];
            var b = list[1];
            var c = list[0];

            var ab = Math.Pow(a, p) * Math.Pow(b, p);
            var ac = Math.Pow(a, p) * Math.Pow(c, p);
            var bc = Math.Pow(b, p) * Math.Pow(c, p);

            return Mathf.PI * 4 * (float)Math.Pow((ab + ac + bc) / 3, 1 / p);
        }

        private bool IsInside(Vector3 point)
        {
            var p = AdjustPoint(point);
            var p2 = new Vector3(p.x * p.x, p.y * p.y, p.z * p.z);

            var x2 = Width * Width;
            var y2 = Height * Height;
            var z2 = Length * Length;

            var total = p2.x / x2 + p2.y / y2 + p2.z / z2 + 0.001f;

            //Debug.Log($"Advanced Shields: Total of {total}.");
            return total < 1;
        }

        private Vector3 AdjustPoint(Vector3 point)
        {
            return Quaternion.Inverse(Rotation) * (point - Position);
        }

        private Vector3 AdjustDirection(Vector3 direction)
        {
            return (Quaternion.Inverse(Rotation) * direction).normalized;
        }
    }
}
