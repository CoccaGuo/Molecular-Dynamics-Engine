using MolecularDynamics.Engine;
using MolecularDynamics.Utils.Vector;
using MolecularDynamics.Utils.Constants;
using MolecularDynamics.Utils.ForceCurve;
using MolecularDynamics.Utils.Generator;
using MolecularDynamics.Utils.Monitor;
using MolecularDynamics.Utils.BoundaryCondition;

namespace MolecularDynamics.Examples.Water
{
    /// <summary>
    /// This is not success for some reason. Anyway, the engine code seems to be OK,
    /// and there is some problem in the force curve or sth like that.
    /// Aborted.
    /// </summary>
    public class TwoDimensionalWater
    {
        public static void Run()
        {
            var world = World.GetWorld();
            var simulator = new Simulator();
            world.AddObject(simulator);
            simulator.Box = new Box(new PeriodicBoundaryConditions(10, 100, 100)); 
            simulator.Initializer = new SingleAtomRandomInitializer(new WaterMolecule(), 20);
            simulator.Integrator = new VelocityVerlet(new MonoatomicWater(simulator.Box)); // define the force field and the integrator
            simulator.TickScale = 2f / NaturalUnits.TimeUnit; // 1fs per tick
            var temperatureMonitor = new TemperatureMonitor(130, 1); // for 2D water.
            simulator.Monitors.Add(temperatureMonitor); // rescale the temperature every 50 ticks, set T=300K
            simulator.Monitors.Add(new ConsoleEnergyMoniter(1)); // print the energy every tick
            simulator.Monitors.Add(new XYZFileRecorder("FreeWater.xyz", 1));
            simulator.Initialize(); // initialize the box with the initializer (random 100 Argon atoms)

            var steps = 50000; 
            for (int i = 0; i < steps; i++)
            {
                world.Tick();
            }

        }
    }

    /// <summary>
    /// Because the usage of mW model, the freedom of degree is still 3 (as an atom).
    /// </summary>
    public class WaterMolecule: IParticle
    {
        public string Name { get; set; } = "Water";
        public float Mass { get; set; } = 18.01528f;
        public float Charge { get; set; } = 0;
        public Vec3 Position { get; set; }
        public Vec3 Velocity { get; set; }
        public bool IsFixed { get; set; } = false;

        public float KineticEnergy() => 0.5f * Mass * Velocity.SqrMagnitude();

        public IParticle Clone() => new WaterMolecule();
        
    }
}