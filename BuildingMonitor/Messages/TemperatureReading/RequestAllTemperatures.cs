namespace BuildingMonitor.Messages.TemperatureReading
{
    public sealed class RequestAllTemperatures
    {
	    public long RequestId { get; }

	    public RequestAllTemperatures(long requestId)
	    {
		    RequestId = requestId;
	    }
    }
}
