using System;
using System.Threading.Tasks;
using Akka.Actor;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages.TemperatureReading;

namespace BuildingMonitor.ConsoleHost
{
    class Program
    {
	    private static void Main(string[] args)
		{
			using (var system = ActorSystem.Create("building-iot-system"))
			{
				var floorManager = system.ActorOf(Props.Create<FloorManagerActor>(), "floors-manager");

				CreateSimulatedSensors(floorManager);

				while (true)
				{
					Console.WriteLine("Press enter to query, Q to quit");

					var cmd = Console.ReadLine();

					if (cmd.ToUpperInvariant() == "Q")
					{
						Environment.Exit(0);
					}

					DisplayTemperatures(system);
				}
			}
		}

		private static void CreateSimulatedSensors(IActorRef floorManager)
		{
			for (var simulatedSensorId = 0; simulatedSensorId < 10; simulatedSensorId++)
			{
				var newSimulatedSensor = new SimulatedSensor("basement", $"{simulatedSensorId}", floorManager);

				newSimulatedSensor.Connect().GetAwaiter().GetResult();

				var simulateNoReadingYet = simulatedSensorId == 3;

				if (!simulateNoReadingYet)
				{
					newSimulatedSensor.StartSendingSimulatedReadings();
				}
			}
		}

		private static void DisplayTemperatures(ActorSystem system)
		{
			var temps = system
				.ActorSelection("akka://building-iot-system/user/floors-manager/floor-basement")
				.Ask<RespondAllTemperatures>(new RequestAllTemperatures(0))
				.Result;

			Console.CursorLeft = 0;
			Console.CursorTop = 0;

			foreach (var temp in temps.TemperatureReadings)
			{
				Console.Write($"Sensor {temp.Key} {temp.Value.GetType().Name}");

				if (temp.Value is TemperatureAvailable available)
				{
					Console.Write($" {available.Temperature:00.00}");
				}

				Console.WriteLine("        ");
			}
		}
	}
}
