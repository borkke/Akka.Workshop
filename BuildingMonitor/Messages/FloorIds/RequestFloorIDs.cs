using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace BuildingMonitor.Messages.FloorIds
{
    public sealed class RequestFloorIds
    {
	    public long RequestId { get; }

	    public RequestFloorIds(long requestId)
	    {
		    RequestId = requestId;
	    }
    }
}
