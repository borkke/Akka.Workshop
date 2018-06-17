namespace BuildingMonitor.Messages.UpdateTemperature
{
	public class RespondTemperatureUpdated
	{
		public long RequestId { get; }

		public RespondTemperatureUpdated(long requestId)
		{
			RequestId = requestId;
		}
	}
}