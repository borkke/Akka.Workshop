namespace BuildingMonitor.Messages.Medatata
{
    public sealed class RequestMetadata
    {
	    public long RequestId { get; }

	    public RequestMetadata(long requestId)
	    {
		    RequestId = requestId;
	    }
    }
}
