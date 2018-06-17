using System.Collections.Immutable;

namespace BuildingMonitor.Messages.SensorIds
{
    public sealed class RespondTemperatureSensorIds
    {
	    public long RequestId { get; }
	    public IImmutableSet<string> SensorIds { get; }

	    public RespondTemperatureSensorIds(long requestId, IImmutableSet<string> sensorIds)
	    {
		    RequestId = requestId;
		    SensorIds = sensorIds;
	    }
    }
}
