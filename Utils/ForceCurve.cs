using MolecularDynamics.Engine;
using MolecularDynamics.Utils.Vector;

namespace MolecularDynamics.Utils.ForceCurve
{
    #region Integrator (Velocity-Verlet)
    class VelocityVerlet : Integrator
    {
        public VelocityVerlet(IForceCurve forceCurve) : base(forceCurve) { }
        public override void Integrate(IParticle particle, float tickScale)
        {
            if (particle.IsFixed) return;
            var halfTickScale = tickScale * 0.5f;
            var tickScaleSqr = tickScale * tickScale;
            var mass = particle.Mass;
            var force = ForceCurve!.GetForce(particle);
            var position = particle.Position;
            var velocity = particle.Velocity;

            velocity += force * halfTickScale / mass;
            position += velocity * tickScale;
            force = ForceCurve.GetForce(particle); // renew the force after position changed
            velocity += force * halfTickScale / mass;

            particle.Velocity = velocity;
            particle.Position = position;
        }
    }
    #endregion


    #region Force Curve (Lennard-Jones)
    class LennardJones : IForceCurve
    {
        public float Epsilon { get; set; }
        public float Sigma { get; set; }
        public float CutOff { get; set; }

        List<IParticle> particles;
        Box box;

        public LennardJones(Box box, float epsilon, float sigma, float cutOff)
        {
            this.box = box;
            this.particles = box.Particles;
            this.Epsilon = epsilon;
            this.Sigma = sigma;
            this.CutOff = cutOff;
        }


        private List<IParticle> FindNearParticles(IParticle particle)
        {
            var _list = new List<IParticle>();
            foreach (var otherParticle in particles)
            {
                if (particle == otherParticle) continue;
                if (Vector.Vector.SqrMagnitude(box.BoundaryConfig.RelativeVector(particle.Position, otherParticle.Position)) < CutOff * CutOff)
                {
                    _list.Add(otherParticle);
                }
            }
            return _list;
        }

        public Vec3 GetForce(IParticle particle)
        {
            var force = new Vec3(0, 0, 0);
            foreach (var otherParticle in FindNearParticles(particle))
            {
                if (particle == otherParticle) continue;
                var r = box.BoundaryConfig.RelativeVector(particle.Position, otherParticle.Position);
                var sr2 = Sigma * Sigma / Vector.Vector.SqrMagnitude(r);
                var sr6 = sr2 * sr2 * sr2;
                var sr12 = sr6 * sr6;
                var f = 24 * Epsilon * (2 * sr12 - sr6) / Vector.Vector.Magnitude(r);
                force = force + r * f;
            }
            return force;
        }
        public float PotentialEnergy(IParticle particle)
        {
            var energy = 0f;
            foreach (var otherParticle in FindNearParticles(particle))
            {
                if (particle == otherParticle) continue;
                var r = box.BoundaryConfig.RelativeVector(particle.Position, otherParticle.Position);
                var sr2 = Sigma * Sigma / Vector.Vector.SqrMagnitude(r);
                var sr6 = sr2 * sr2 * sr2;
                var sr12 = sr6 * sr6;
                energy += 2 * Epsilon * (sr12 - sr6); // half due to energy is contained in both particles
            }
            return energy;
        }
    }
    #endregion

    #region Force Curve (mW)
    /// <summary>
    /// https://pubs.acs.org/doi/10.1021/jp805227c
    /// </summary>
    public class MonoatomicWater : IForceCurve
    {
        public float Epsilon { get; set; } = 0.2685f; //eV
        public float Sigma { get; set; } = 2.3925f; //Angstrom
        public float Lambda { get; set; } = 23.15f;
        public float CutOff { get; set; } = 4.32f; //Angstrom

        private float A = 7.049556277f;
        private float B = 0.6022245584f;
        private float gamma = 1.2f;
        private float a = 1.8f;
        private float theta0 = 1.910612f; // 109.47 degree


        List<IParticle> particles;
        Box box;

        private float E(float x) => (float)Math.Exp(x);
        private float Sqr(float x) => x * x;
        private float Pow4(float x) => Sqr(Sqr(x));
        private float fE(float x) => E(gamma*Sigma/(x-a*Sigma));
        private float Cmsq(float th) => Sqr((float)(Math.Cos(th) - Math.Cos(theta0)));


        public MonoatomicWater(Box box)
        {
            this.box = box;
            this.particles = box.Particles;
        }

        private List<IParticle> FindNearParticles(IParticle particle)
        {
            var _list = new List<IParticle>();
            foreach (var otherParticle in particles)
            {
                if (particle == otherParticle) continue;
                if (Vector.Vector.SqrMagnitude(box.BoundaryConfig.RelativeVector(particle.Position, otherParticle.Position)) < CutOff * CutOff)
                {
                    _list.Add(otherParticle);
                }
            }
            return _list;
        }

        private Vec3 SinglePhi2Force(IParticle p1, IParticle p2)
        {
            var rRel = box.BoundaryConfig.RelativeVector(p1.Position, p2.Position);
            var r = rRel.Magnitude();
            var term1 = - (A*Epsilon*Sigma*E(Sigma/(r-a*Sigma))*(B*Pow4(Sigma)/Pow4(r)-1))
                            / Sqr(r-a*Sigma);
            var term2 = -(4 * A * B * Epsilon * Pow4(Sigma) * E(Sigma / (r - a * Sigma)))
                            / Pow4(r) / r;
            return rRel.Unit() * (term1 + term2);
        }

        private Vec3 Phi2Force(IParticle particle, List<IParticle> nearParticles)
        {
            var force = new Vec3(0, 0, 0);
            foreach (var otherParticle in nearParticles)
            {
                if (particle == otherParticle) continue;
                force += SinglePhi2Force(particle, otherParticle);
            }
            return force;
        }

        private Vec3 SinglePhi3Force(IParticle center, IParticle p1, IParticle p2)
        {
            var rRel = box.BoundaryConfig.RelativeVector(center.Position, p1.Position);
            var sRel = box.BoundaryConfig.RelativeVector(center.Position, p2.Position);
            var r = Vector.Vector.Magnitude(rRel);
            var s = Vector.Vector.Magnitude(sRel);
            var theta = Vector.Vector.Angle(rRel, sRel);
            var forceCore = -(Epsilon * gamma * Lambda * Sigma * fE(r) * fE(s) * Cmsq(theta));
            var forceR = rRel.Unit()* forceCore / Sqr(r - a * Sigma);
            var forceS = sRel.Unit()* forceCore / Sqr(s - a * Sigma);
            var forceThetaDirect = (rRel.Unit() + sRel.Unit()).Unit();
            var forceTheta = forceThetaDirect*(float)(-2 * Epsilon * Lambda * fE(r) * fE(s) * Math.Sin(theta) * Math.Sqrt(Cmsq(theta)));
            return forceR + forceS + forceTheta;

        }

        private Vec3 Phi3Force(IParticle particle, List<IParticle> nearParticles)
        {
            var force = new Vec3(0, 0, 0);
            // find all possible combanations of 3 particles
            foreach (var p1 in nearParticles)
            {
                if (particle == p1) continue;
                foreach (var p2 in nearParticles)
                {
                    if (particle == p2 || p1 == p2) continue;
                    force += SinglePhi3Force(particle, p1, p2);
                }
            }
            return force;
        }

        public Vec3 GetForce(IParticle particle)
        {
            var force = new Vec3(0, 0, 0);
            var nearParticles = FindNearParticles(particle);
            force += Phi2Force(particle, nearParticles);
            force += Phi3Force(particle, nearParticles);
            return force;
        }

        private float SinglePhi2Potential(IParticle p1, IParticle p2)
        {
            var rRel = box.BoundaryConfig.RelativeVector(p1.Position, p2.Position);
            var r = Vector.Vector.Magnitude(rRel);
            var Sigma4 = Sigma * Sigma * Sigma * Sigma;
            float part1 = A * Epsilon;
            float part2 = (float)((B * Sigma4 / r / r / r / r - 1) * Math.Exp((Sigma / (r - a * Sigma))));
            return part1 * part2 / 2;
        }

        private float SinglePhi3Potential(IParticle center, IParticle p1, IParticle p2)
        {
            var rRel = box.BoundaryConfig.RelativeVector(center.Position, p1.Position);
            var sRel = box.BoundaryConfig.RelativeVector(center.Position, p2.Position);
            var r = Vector.Vector.Magnitude(rRel);
            var s = Vector.Vector.Magnitude(sRel);
            var theta = Vector.Vector.Angle(rRel, sRel);
            float term1 = Lambda * Epsilon * (float)Math.Pow(Math.Cos(theta) - Math.Cos(theta0), 2);
            float termExp(float x) => (float)Math.Exp(gamma * Sigma / (x - a * Sigma));
            return term1 * termExp(r) * termExp(s);
        }


        public float PotentialEnergy(IParticle particle)
        {
            var energy = 0f;
            var nearParticles = FindNearParticles(particle);
            foreach (var otherParticle in nearParticles)
            {
                if (particle == otherParticle) continue;
                energy += SinglePhi2Potential(particle, otherParticle);
                // 3-body potentials
                foreach (var p1 in nearParticles)
                {
                    if (particle == p1 || otherParticle == p1) continue;
                    energy += SinglePhi3Potential(particle, otherParticle, p1);
                }
            }

            return energy;
        }
    }

    #endregion

}