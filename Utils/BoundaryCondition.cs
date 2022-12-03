using MolecularDynamics.Engine;
using MolecularDynamics.Utils.Vector;

namespace MolecularDynamics.Utils.BoundaryCondition
{
    /// 满足最小镜像原则的3D周期边界条件
    public class PeriodicBoundaryConditions : IBoundaryConfig
    {
        public int X { get; set; } // x
        public int Y { get; set; } // y
        public int Z { get; set; } // z

        /// <summary>
        ///     Boundary Range: 0 ~ x, 0 ~ y, 0 ~ z
        /// </summary>
        public PeriodicBoundaryConditions(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        ///     Born-von Karman Boundary Condition
        /// </summary>
        public Vec3 Wrap(Vec3 vec)
        {
            var x = vec.X;
            var y = vec.Y;
            var z = vec.Z;
            while (!IsInside(x, y, z))
            {
                if (x < 0) x += X;
                if (x > X) x -= X;
                if (y < 0) y += Y;
                if (y > Y) y -= Y;
                if (z < 0) z += Z;
                if (z > Z) z -= Z;
            }
            return new Vec3(x, y, z);
        }

        bool IsInside(float x, float y, float z)
        {
            return x >= 0 && x <= X && y >= 0 && y <= Y && z >= 0 && z <= Z;
        }

        /// <summary>
        ///    Relative vector by minimum image convention.
        /// </summary>
        public Vec3 RelativeVector(Vec3 a, Vec3 b)
        {
            var x = a.X - b.X;
            var y = a.Y - b.Y;
            var z = a.Z - b.Z;
            if (x > X / 2) x -= X;
            if (x < -X / 2) x += X;
            if (y > Y / 2) y -= Y;
            if (y < -Y / 2) y += Y;
            if (z > Z / 2) z -= Z;
            if (z < -Z / 2) z += Z;
            return new Vec3(x, y, z);
        }

        public float Volume => X * Y * Z;
    }
}