using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using BuildingMonitor.Messages;
using BuildingMonitor.Messages.RegisterSensor;

namespace BuildingMonitor.ConsoleHost
{
    public class SimulatedSensor
    {
	    private IActorRef _floorManager;
	    private readonly Random _randomTemperatureGenerator;
	    private IActorRef _sensorRef;
	    private Timer _timer;
	    private readonly string _floorId;
	    private readonly string _sensorId;

		public SimulatedSensor(string floorId, string sensorId, IActorRef floorManager)
	    {
		    _floorId = floorId;
		    _sensorId = sensorId;
		    _floorManager = floorManager;
		    _randomTemperatureGenerator = new Random(int.Parse(sensorId));
	    }

	    public async Task Connect()
	    {
		    var response = await _floorManager.Ask<RespondRegisterTemperatureSensor>(
			    new RequestRegisterTemperatureSensor(1, _floorId, _sensorId));

		    _sensorRef = response.SensorRef;
	    }

	    public void StartSendingSimulatedReadings()
	    {
		    _timer = new Timer(SimulateUpdateTemperature, null, 0, 1000);
	    }

		private void SimulateUpdateTemperature(object sender)
		{
			var randomTemperature = _randomTemperatureGenerator.NextDouble();

			randomTemperature *= 100;

			_sensorRef.Ask(new RequestUpdateTemperature(0, randomTemperature));
		}
	}
}
