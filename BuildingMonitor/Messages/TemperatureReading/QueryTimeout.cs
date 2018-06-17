namespace BuildingMonitor.Messages.TemperatureReading
{
    public sealed class QueryTimeout
    {
	    public static QueryTimeout Instance { get; } = new QueryTimeout();
	    private QueryTimeout() {}
    }
}
