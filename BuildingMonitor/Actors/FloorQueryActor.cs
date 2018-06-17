using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Akka.Actor;
using BuildingMonitor.Messages;
using BuildingMonitor.Messages.TemperatureReading;

namespace BuildingMonitor.Actors
{
    public class FloorQueryActor : UntypedActor
    {
		public static readonly long TemperatureRequestCorrelationId = 42;

		private Dictionary<IActorRef, string> _actorToSensorId;
		private long _requestId;
		private IActorRef _requester;
		private TimeSpan _timeout;
	    private ICancelable _queryTimeoutTimer;

		private Dictionary<string, ITemperatureQueryReading> _repliesReceived = new Dictionary<string, ITemperatureQueryReading>();
		private HashSet<IActorRef> _stillAwaitingReply;


		public FloorQueryActor(Dictionary<IActorRef, string> actorToSensorId,
						  long requestId,
						  IActorRef requester,
						  TimeSpan timeout)
		{
			_actorToSensorId = actorToSensorId;
			_requestId = requestId;
			_requester = requester;
			_timeout = timeout;

			_stillAwaitingReply = new HashSet<IActorRef>(_actorToSensorId.Keys);
			_queryTimeoutTimer = Context.System.Scheduler.ScheduleTellOnceCancelable(
				timeout, Self, QueryTimeout.Instance, Self);
		}

		protected override void PreStart()
		{
			foreach (var temperatureSensor in _actorToSensorId.Keys)
			{
				Context.Watch(temperatureSensor);
				temperatureSensor.Tell(new RequestTemperature(TemperatureRequestCorrelationId));
			}
		}

	    protected override void PostStop()
	    {
		    _queryTimeoutTimer.Cancel();
	    }

	    protected override void OnReceive(object message)
		{
			switch (message)
			{
				case RespondTemperature m when m.RequestId == TemperatureRequestCorrelationId:
					ITemperatureQueryReading reading = null;
					if (m.Temperature.HasValue)
					{
						reading = new TemperatureAvailable(m.Temperature.Value);
					}
					else
					{
						reading = NoTemperatureReadingRecordedYet.Instance;
					}
					RecordSensorResponse(Sender, reading);
					break;
				case QueryTimeout m:
					foreach (var sensor in _stillAwaitingReply)
					{
						var sensorId = _actorToSensorId[sensor];
						_repliesReceived.Add(sensorId, TemperatureSensorTimedOut.Instance);
					}
					_requester.Tell(new RespondAllTemperatures(
						_requestId, _repliesReceived.ToImmutableDictionary()));
					Context.Stop(Self);
					break;
				case Terminated m:
					RecordSensorResponse(m.ActorRef,TemperatureSensorNotAvailable.Instance);
					break;
				default:
					Unhandled(message);
					break;
			}
		}

		private void RecordSensorResponse(IActorRef sensorActor,
										  ITemperatureQueryReading reading)
		{
			Context.Unwatch(sensorActor);

			var sensorId = _actorToSensorId[sensorActor];

			_stillAwaitingReply.Remove(sensorActor);
			_repliesReceived.Add(sensorId, reading);

			var allRepliesHaveBeenRecieved = _stillAwaitingReply.Count == 0;

			if (allRepliesHaveBeenRecieved)
			{
				_requester.Tell(new RespondAllTemperatures(
					_requestId,
					_repliesReceived.ToImmutableDictionary()));
				Context.Stop(Self);
			}
		}

		public static Props Props(Dictionary<IActorRef, string> actorToSensorId,
								  long requestId,
								  IActorRef requester,
								  TimeSpan timeout) =>
					Akka.Actor.Props.Create(() =>
							new FloorQueryActor(actorToSensorId, requestId, requester, timeout));
	}
}
