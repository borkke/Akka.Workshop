using System.Collections.Immutable;

namespace BuildingMonitor.Messages.FloorIds
{
    public sealed class RespondFloorIds
    {
	    public ImmutableHashSet<string> FloorIds { get; }
	    public long RequestId { get; }

	    public RespondFloorIds(long requestId, ImmutableHashSet<string> floorIds)
	    {
		    FloorIds = floorIds;
		    RequestId = requestId;
	    }
    }
}
