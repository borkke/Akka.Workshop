using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Akka.Actor;
using BuildingMonitor.Messages.RegisterSensor;
using BuildingMonitor.Messages.SensorIds;
using BuildingMonitor.Messages.TemperatureReading;

namespace BuildingMonitor.Actors
{
    public class FloorActor :UntypedActor
    {
	    private readonly string _floorId;
	    private readonly Dictionary<string, IActorRef> _sensorsMap = new Dictionary<string, IActorRef>();

	    public FloorActor(string floorId)
	    {
		    _floorId = floorId;
	    }

	    protected override void OnReceive(object message)
	    {
		    switch (message)
		    {
				case RequestRegisterTemperatureSensor m when m.FloorId == _floorId:
					if (_sensorsMap.TryGetValue(m.SensorId, out var existingSensorActorRef))
					{
						existingSensorActorRef.Forward(m);
					}
				    else
				    {

					    var newSensor = Context
						    .ActorOf(TemperatureSensorActor.Props(_floorId, m.SensorId), $"temperature-sensor-{m.SensorId}");
					    Context.Watch(newSensor);
					    _sensorsMap.Add(m.SensorId, newSensor);
					    newSensor.Forward(m);
				    }
					break;
				case RequestTemperatureSensorIds m:
					Sender.Tell(new RespondTemperatureSensorIds(m.RequestId, ImmutableHashSet.CreateRange(_sensorsMap.Keys)));
					break;
				case Terminated m:
					var terminatedSensorId = _sensorsMap.First(a => a.Value.Equals(m.ActorRef)).Key;
					_sensorsMap.Remove(terminatedSensorId);
					break;
				case RequestAllTemperatures m:
					var map = new Dictionary<IActorRef, string>();
					foreach (var item in _sensorsMap)
					{
						map.Add(item.Value, item.Key);
					}
					Context.ActorOf(FloorQueryActor.Props(map,
						m.RequestId,
						Sender,
						TimeSpan.FromSeconds(3)));
					break;
				default:
					Unhandled(message);
					break;
		    }
	    }

		public static Props Props(string floorId) =>
			Akka.Actor.Props.Create(() => new FloorActor(floorId));
	}
}
