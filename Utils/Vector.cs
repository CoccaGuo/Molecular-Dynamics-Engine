namespace MolecularDynamics.Utils.Vector
{
    public struct Vec3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator *(Vec3 a, Vec3 b) => new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vec3 operator /(Vec3 a, Vec3 b) => new Vec3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        public static Vec3 operator +(Vec3 a, float b) => new Vec3(a.X + b, a.Y + b, a.Z + b);
        public static Vec3 operator -(Vec3 a, float b) => new Vec3(a.X - b, a.Y - b, a.Z - b);
        public static Vec3 operator *(Vec3 a, float b) => new Vec3(a.X * b, a.Y * b, a.Z * b);
        public static Vec3 operator /(Vec3 a, float b) => new Vec3(a.X / b, a.Y / b, a.Z / b);

        public float SqrMagnitude() => X * X + Y * Y + Z * Z;

        public float Magnitude() => (float)Math.Sqrt(SqrMagnitude());

        public Vec3 Unit() => this / Magnitude(); 
        
    }


    public static class Vector
    {
        public static Vec3 Add(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 Subtract(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 Multiply(Vec3 a, Vec3 b) => new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vec3 Divide(Vec3 a, Vec3 b) => new Vec3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        public static Vec3 Add(Vec3 a, float b) => new Vec3(a.X + b, a.Y + b, a.Z + b);
        public static Vec3 Subtract(Vec3 a, float b) => new Vec3(a.X - b, a.Y - b, a.Z - b);
        public static Vec3 Multiply(Vec3 a, float b) => new Vec3(a.X * b, a.Y * b, a.Z * b);
        public static Vec3 Divide(Vec3 a, float b) => new Vec3(a.X / b, a.Y / b, a.Z / b);
        public static float Magnitude(Vec3 a) => (float)Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        public static Vec3 Normalize(Vec3 a) => Divide(a, Magnitude(a));
        public static float Distance(Vec3 a, Vec3 b) => Magnitude(Subtract(a, b));
        public static float Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Vec3 Cross(Vec3 a, Vec3 b) => new Vec3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

        public static float Angle(Vec3 a, Vec3 b) => (float)Math.Acos(Dot(a, b) / (Magnitude(a) * Magnitude(b)));
        public static float SqrMagnitude(Vec3 a) => a.X * a.X + a.Y * a.Y + a.Z * a.Z;
    }


}