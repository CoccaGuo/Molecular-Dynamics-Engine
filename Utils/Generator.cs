using MolecularDynamics.Engine;
using MolecularDynamics.Utils.Vector;

namespace MolecularDynamics.Utils.Generator
{
    public class SingleAtomRandomInitializer: Initializer
    {   
        public IParticle Base { get; set; }
        public int Count { get; set; }

        public SingleAtomRandomInitializer(IParticle _base, int count)
        {
            Base = _base;
            Count = count;
        }
        
        public override void Initialize(Box box)
        {
            var particles = box.Particles;
            var random = new System.Random();

            var refLength = Math.Pow(box.BoundaryConfig.Volume, 1f/3f);
            for (int i = 0; i < Count; i++)
            {   
                var particle = Base.Clone();

                particle.Position = new Vec3(
                    (float)(random.NextDouble() * refLength),
                    (float)(random.NextDouble() * refLength),
                    (float)(random.NextDouble() * refLength)
                );
                particle.Velocity = new Vec3(
                    (float)random.NextDouble() * 2 - 1,
                    (float)random.NextDouble() * 2 - 1,
                    (float)random.NextDouble() * 2 - 1
                );

                particles.Add(particle);
            }
        }
    }
}