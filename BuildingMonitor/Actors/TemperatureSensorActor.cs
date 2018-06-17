using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using BuildingMonitor.Messages;
using BuildingMonitor.Messages.Medatata;
using BuildingMonitor.Messages.RegisterSensor;
using BuildingMonitor.Messages.UpdateTemperature;

namespace BuildingMonitor.Actors
{
    public class TemperatureSensorActor : UntypedActor
    {
	    private readonly string _floorId;
	    private readonly string _sensorId;
	    private double? _lastRecordedTemperature;

	    public TemperatureSensorActor(string floorId, string sensorId)
	    {
		    _floorId = floorId;
		    _sensorId = sensorId;
	    }

	    protected override void OnReceive(object message)
	    {
		    switch (message)
		    {
				case RequestMetadata m:
					Sender.Tell(new RespondMetadata(m.RequestId, _floorId, _sensorId));
					break;
				case RequestTemperature m:
					Sender.Tell(new RespondTemperature(m.RequestId, _lastRecordedTemperature));
					break;
				case RequestUpdateTemperature m:
					_lastRecordedTemperature = m.Temperature;
					Sender.Tell(new RespondTemperatureUpdated(m.RequestId));
					break;
				case RequestRegisterTemperatureSensor m when
					m.FloorId == _floorId && m.SensorId == _sensorId:
					Sender.Tell(new RespondRegisterTemperatureSensor(m.RequestId, Context.Self));
					break;
			    default:
					Unhandled(message);
					break;
		    }
	    }

	    public static Props Props(string floor, string sensorId) => 
			Akka.Actor.Props.Create(() => new TemperatureSensorActor(floor, sensorId));
    }
}
