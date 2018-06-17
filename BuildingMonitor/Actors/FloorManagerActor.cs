using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Akka.Actor;
using BuildingMonitor.Messages.FloorIds;
using BuildingMonitor.Messages.RegisterSensor;

namespace BuildingMonitor.Actors
{
    public class FloorManagerActor : UntypedActor
    {
	    private readonly Dictionary<string, IActorRef> _floorMap = new Dictionary<string, IActorRef>();


		protected override void OnReceive(object message)
	    {
		    switch (message)
		    {
			    case RequestRegisterTemperatureSensor m :
				    if (_floorMap.TryGetValue(m.FloorId, out var existingFloorActorRef))
				    {
					    existingFloorActorRef.Forward(m);
				    }
				    else
				    {

					    var newFloor = Context.ActorOf(FloorActor.Props(m.FloorId), $"floor-{m.FloorId}");
						_floorMap.Add(m.FloorId, newFloor);
					    Context.Watch(newFloor);
					    newFloor.Forward(m);
				    }
				    break;
				case RequestFloorIds m:
					Sender.Tell(new RespondFloorIds(m.RequestId, ImmutableHashSet.CreateRange(_floorMap.Keys)));
					break;
				case Terminated m:
					var terminatedFloorId = _floorMap.First(a => a.Value == m.ActorRef).Key;
					_floorMap.Remove(terminatedFloorId);
					break;
				default:
					Unhandled(message);
					break;
		    }
	    }

	    public static Props Props() => 
		    Akka.Actor.Props.Create<FloorManagerActor>();
    }
}
