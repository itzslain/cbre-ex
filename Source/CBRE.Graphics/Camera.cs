using CBRE.Graphics.Helpers;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace CBRE.Graphics
{
    public class Camera
    {
        public Vector3 LookAt { get; set; }
        public Vector3 Location { get; set; }
        public int FOV { get; set; }
        public int ClipDistance { get; set; }

        public Camera()
        {
            LookAt = new Vector3(1, 0, 0);
            Location = new Vector3(0, 0, 0);
            FOV = 60;
            ClipDistance = 4000;
        }

        public void Position()
        {
            MatrixMode mode = Matrix.CurrentMode;
            Matrix.Set(MatrixMode.Modelview);
            Matrix4 modelview = Matrix4.LookAt(Location, LookAt, Vector3.UnitZ);
            GL.LoadMatrix(ref modelview);
            Matrix.Set(mode);
        }

        public decimal GetRotation()
        {
            Vector3 temp = (LookAt - Location);
            temp.Normalize();
            double rot = Math.Atan2(temp.Y, temp.X);
            if (rot < 0) rot += 2 * Math.PI;
            if (rot > 2 * Math.PI) rot = rot % (2 * Math.PI);
            return (decimal)rot;
        }

        public void SetRotation(decimal rotation)
        {
            Vector3 temp = (LookAt - Location);
            temp.Normalize();
            decimal e = GetElevation();
            double x = Math.Cos((double)rotation) * Math.Sin((double)e);
            double y = Math.Sin((double)rotation) * Math.Sin((double)e);
            LookAt = new Vector3((float)x + Location.X, (float)y + Location.Y, temp.Z + Location.Z);
        }

        public decimal GetElevation()
        {
            Vector3 temp = (LookAt - Location);
            temp.Normalize();
            double elev = Math.Acos(temp.Z);
            return (decimal)elev;
        }

        public void SetElevation(decimal elevation)
        {
            if (elevation > ((decimal)Math.PI * 0.99m)) elevation = (decimal)Math.PI * 0.99m;
            if (elevation < ((decimal)Math.PI * 0.01m)) elevation = (decimal)Math.PI * 0.01m;
            decimal rotation = GetRotation();
            double x = Math.Cos((double)rotation) * Math.Sin((double)elevation);
            double y = Math.Sin((double)rotation) * Math.Sin((double)elevation);
            double z = Math.Cos((double)elevation);
            LookAt = new Vector3((float)x + Location.X, (float)y + Location.Y, (float)z + Location.Z);
        }

        public void Pan(decimal degrees)
        {
            decimal rad = degrees * ((decimal)Math.PI / 180);
            decimal rot = GetRotation();
            SetRotation(rot + rad);
        }

        public void Tilt(decimal degrees)
        {
            SetElevation(GetElevation() + (degrees * ((decimal)Math.PI / 180)));
        }

        public void Advance(decimal units)
        {
            Vector3 temp = LookAt - Location;
            temp.Normalize();
            Vector3 add = temp * (float)units;
            LookAt += add;
            Location += add;
        }

        public void Strafe(decimal units)
        {
            Vector3 right = GetRight();
            Vector3 add = right * (float)units;
            LookAt += add;
            Location += add;
        }

        public void Ascend(decimal units)
        {
            Vector3 up = GetUp();
            Vector3 add = up * (float)units;
            LookAt += add;
            Location += add;
        }

        public void AscendAbsolute(decimal units)
        {
            Vector3 up = new Vector3(0, 0, (float)units);
            LookAt += up;
            Location += up;
        }

        public Vector3 GetUp()
        {
            Vector3 temp = LookAt - Location;
            temp.Normalize();
            Vector3 normal = Vector3.Cross(GetRight(), temp);
            normal.Normalize();
            return normal;
        }

        public Vector3 GetRight()
        {
            Vector3 temp = LookAt - Location;
            temp.Z = 0;
            temp.Normalize();
            Vector3 normal = Vector3.Cross(temp, Vector3.UnitZ);
            normal.Normalize();
            return normal;
        }
    }
}
