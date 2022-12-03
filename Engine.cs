using MolecularDynamics.Utils.Vector;

namespace MolecularDynamics.Engine
{
    public interface ITickable
    {
        void Tick();
    }

    /// <summary>
    /// 单态类World，因为只有一个世界。世界是有时间序列的，故继承ITickable接口。
    /// 可以把有时间序列的东西放到世界里，这些东西可以随时间演化。
    /// 一般，我们把Simulator放进世界里。
    /// gives control of the time (sequencing) of the simulation。
    /// Only one instance as the world is a singleton (even in real world!)
    /// </summary>
    public class World : ITickable
    {
        private static World? _instance = null;
        private World() => Tickables = new List<ITickable>();
        public List<ITickable> Tickables { get; set; }
        public static World GetWorld() => _instance ??= new World();
        public void AddObject(ITickable tickable) => Tickables.Add(tickable);
        public void RemoveObject(ITickable tickable) => Tickables.Remove(tickable);

        public void Tick()
        {
            foreach (var tickable in Tickables) tickable.Tick();
        }
    }
    /// <summary>
    /// 一个MD模拟器。
    /// </summary>
    public class Simulator : ITickable
    {
        public Box? Box { get; set; }
        public Initializer? Initializer { get; set; }
        public Integrator? Integrator { get; set; }
        public List<IMonitor> Monitors { get; set; } = new List<IMonitor>();

        public float TickScale { get; set; }
        public long TickCount { get; set; } = 0;

        public void Initialize()
        {
            Initializer?.Initialize(Box!);
        }

        public void Tick()
        {
            foreach (var particle in Box!.Particles!) Integrator?.Integrate(particle, TickScale);
            foreach (var monitor in Monitors) monitor.Rescale(this, TickCount);
            Box.Wrap();
            TickCount++;
        }
    }

    #region IMonitor
    /// <summary>
    /// 继承这个接口可以在运行时监视、记录、调整粒子的速度、位置等。
    /// </summary>
    public interface IMonitor
    {
        void Rescale(Simulator sim, float tickCount = 0);
    }

    
    #endregion

    #region Initializer
    /// <summary>
    /// 初始化器的抽象。初始化粒子位置、速度、种类。
    /// </summary>
    public abstract class Initializer
    {
        public abstract void Initialize(Box box);
    }
    #endregion

    #region Integrator
    /// <summary>
    /// 积分器的抽象。任何积分器都需要用一个Force Curve更新粒子状态。
    /// </summary>
    public abstract class Integrator
    {
        public IForceCurve? ForceCurve { get; set; }
        public Integrator(IForceCurve forceCurve) => ForceCurve = forceCurve;
        public abstract void Integrate(IParticle particle, float dt);

    }
    #endregion


    #region IForceCurve
    /// <summary>
    /// Force Curve的抽象。Force Curve 应给出单个粒子的受力（相对于其他所有粒子），以及该粒子蕴含的势能。
    /// </summary>
    public interface IForceCurve
    {  
        Vec3 GetForce(IParticle particle);
        public float PotentialEnergy(IParticle particle); // partial energy (e.g. half in 2-body interactions)
    }
    #endregion

    #region IParticle
    // The interface of particles
    // 定义粒子性质用的接口，粒子应有质量、位置、速度、动能等属性。
    public interface IParticle
    {   
        public string Name { get; set; }
        public float Mass { get; set; }
        public float Charge { get; set; }
        public Vec3 Position { get; set; }
        public Vec3 Velocity { get; set; }

        public bool IsFixed { get; set; }

        public float KineticEnergy();
        public IParticle Clone();
    }
    #endregion

    #region Box
    /// A Container that holds a collection of particles, with a certain boundary configuration.
    /// 一个含有一些粒子的盒子。这个盒子只处理位置相关信息，不负责更新粒子的速度。
    public class Box
    {
        public List<IParticle> Particles { get; set; }
        public IBoundaryConfig BoundaryConfig { get; set; }
        public Box(IBoundaryConfig boundaryConfig)
        {
            Particles = new List<IParticle>();
            BoundaryConfig = boundaryConfig;
        }

        public void AddParticle(IParticle particle) => Particles?.Add(particle);
        public void RemoveParticle(IParticle particle) => Particles?.Remove(particle);
        public void Wrap()
        {
            foreach (var particle in Particles!)
            {
                particle.Position = BoundaryConfig.Wrap(particle.Position); // make sure the pos meets the rule of BoundaryConfig
            }
        }

    }
    #endregion

    #region IBoundaryConfig

    /// gives control of the configuration space of the simulation.
    /// 这个接口用来定义一定的边界条件
    public interface IBoundaryConfig
    {
        /// to arrange position of particles in the box in a certain rule.
        /// 用来调整粒子在盒子中的位置
        public Vec3 Wrap(Vec3 position);

        /// to get the distance between two particles in the box
        /// 用来获取两个粒子之间的距离
        public Vec3 RelativeVector(Vec3 a, Vec3 b);

        public float Volume { get; }
    }
    #endregion

}