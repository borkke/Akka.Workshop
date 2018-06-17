using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;

namespace BuildingMonitor.Messages.RegisterSensor
{
    public sealed class RespondRegisterTemperatureSensor
    {
	    public long RequestId { get; }
	    public IActorRef SensorRef { get; }

	    public RespondRegisterTemperatureSensor(long requestId, IActorRef sensorRef)
	    {
		    RequestId = requestId;
		    SensorRef = sensorRef;
	    }
    }
}
