using MolecularDynamics.Engine;
using MolecularDynamics.Utils.Constants;

namespace MolecularDynamics.Utils.Monitor
{
  # region Energy & Temperature Console Monitor 

    /// <summary>
    /// Use this to print info into screen, when doing calc.
    /// </summary>
    public class ConsoleEnergyMoniter : IMonitor
    {
        public int RescalePeriod { get; set; }
        public ConsoleEnergyMoniter(int rescalePeriod = 100)
        {
            RescalePeriod = rescalePeriod;
        }
        public void Rescale(Simulator simulator, float tickCount = 0)
        {
            if (tickCount % RescalePeriod == 0)
            {
                var box = simulator.Box;
                var Kenergy = 0f;
                var Uenergy = 0f;
                foreach (var particle in box!.Particles!)
                {
                    Kenergy += particle.KineticEnergy();
                    Uenergy += simulator.Integrator!.ForceCurve!.PotentialEnergy(particle);
                }
                System.Console.WriteLine($"Tick: {tickCount}, Etot: {Kenergy + Uenergy}, Ek: {Kenergy}, Ep: {Uenergy}, T: {Kenergy * 2 / (3 * NaturalUnits.BoltzmannConstant * box.Particles!.Count)}");
            }
        }
    }

    #endregion

    #region Temperature Monitor
    /// <summary>
    /// rescaling Temperature to set point.
    /// </summary>
    public class TemperatureMonitor : IMonitor
    {
        public float Temperature { get; set; }
        public int RescalePeriod { get; set; }
        public TemperatureMonitor(float temperature, int rescalePeriod = 1)
        {
            Temperature = temperature;
            RescalePeriod = rescalePeriod;
        }
        public void Rescale(Simulator sim, float tickCount = 0)
        {      
            var box = sim.Box;
            if (tickCount % RescalePeriod != 0) return;
            var totalKineticEnergy = 0f;
            foreach (var particle in box!.Particles!)
                totalKineticEnergy += particle.KineticEnergy();
            var currentTemperature = totalKineticEnergy * 2 / (3 * NaturalUnits.BoltzmannConstant * box.Particles!.Count);
            var scale = (float)Math.Sqrt(Temperature / currentTemperature);
            foreach (var particle in box.Particles!)
                particle.Velocity = Vector.Vector.Multiply(particle.Velocity, scale);
        }
    }

    #endregion

    #region .xyz file recorder
    /// <summary>
    /// record the structure during the MD.
    /// </summary>
    public class XYZFileRecorder: IMonitor
    {
        public int RescalePeriod { get; set; }
        public string Filename;

        public XYZFileRecorder(string filename, int rescalePeriod = 100)
        {
            RescalePeriod = rescalePeriod;
            Filename = filename;
        }
        public void Rescale(Simulator simulator, float tickCount = 0)
        {
            if (tickCount % RescalePeriod == 0)
            {
                var box = simulator.Box;
                var particles = box!.Particles!;
                // record the positions of the particles in file format of .xyz
                using StreamWriter file = new($"./{Filename}", append: true);
                file.WriteLine(particles.Count);
                file.WriteLine($"tick {tickCount}");
                foreach (var particle in particles)
                    file.WriteLine($"{particle.Name} {particle.Position.X} {particle.Position.Y} {particle.Position.Z}");
                file.Flush();
                file.Dispose();
            }
        }
    }

    #endregion
}