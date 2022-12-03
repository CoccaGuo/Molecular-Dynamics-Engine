using MolecularDynamics.Engine;
using MolecularDynamics.Utils.Constants;
using MolecularDynamics.Utils.ForceCurve;
using MolecularDynamics.Utils.Generator;
using MolecularDynamics.Utils.Monitor;
using MolecularDynamics.Utils.Vector;
using MolecularDynamics.Utils.BoundaryCondition;

namespace MolecularDynamics.Examples.Argon
{

    /// <summary>
    /// This is a simple example of how to use the MolecularDynamics.Engine namespace.
    /// use natural units (eV, Angstrom, fs, amu)
    /// </summary>
    public class Argon
    {
        public static void Run()
        {
            var world = World.GetWorld();
            var simulator = new Simulator();
            world.AddObject(simulator);
            simulator.Box = new Box(new PeriodicBoundaryConditions(30, 30, 30)); // 30 Angstroms in each direction
            simulator.Initializer = new SingleAtomRandomInitializer(new ArgonAtom(), 100); // random 100 Argon atoms
            simulator.Integrator = new VelocityVerlet(new LennardJones(simulator.Box, 
                                        epsilon:0.0103f, sigma: 3.405f, cutOff: 5f)); // define the force field and the integrator
            simulator.TickScale = 5f / NaturalUnits.TimeUnit; // 5fs per tick
            var temperatureMonitor = new TemperatureMonitor(300, 50);
            simulator.Monitors.Add(temperatureMonitor); // rescale the temperature every 50 ticks, set T=300K
            simulator.Monitors.Add(new ConsoleEnergyMoniter(1000)); // print the energy every tick
            simulator.Monitors.Add(new XYZFileRecorder("Argon.xyz", 50));
            simulator.Initialize(); // initialize the box with the initializer (random 100 Argon atoms)

            var steps = 20000;
            for (int i = 0; i < steps; i++)
            {
                world.Tick();

                //start to cool down the argon gas
                if (i % 10000 == 0 && temperatureMonitor.Temperature > 10)
                {
                   // temperatureMonitor.Temperature -= 10;
                }
            }
        }
    }


    class ArgonAtom : IParticle
    {   
        public string Name {get; set;} = "Ar";
        public float Mass { get; set; } = 39.948f; // amu
        public float Charge { get; set; } = 0f; // e
        public Vec3 Position { get; set; } = new Vec3(0, 0, 0);
        public Vec3 Velocity { get; set; } = new Vec3(0, 0, 0);

        public bool IsFixed { get; set; } = false;
        public IParticle Clone() => new ArgonAtom();
        public float KineticEnergy() => 0.5f * Mass * Vector.SqrMagnitude(Velocity);
    }

    
}