using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages.FloorIds;
using BuildingMonitor.Messages.RegisterSensor;
using Xunit;

namespace BuildingMonitorTests
{
    public class FloorManagerActorShould : TestKit
    {
	    [Fact]
	    public void ReturnNoFloorIdsWhenNewlyCreated()
	    {
		    var probe = CreateTestProbe();
		    var manager = Sys.ActorOf(FloorManagerActor.Props());

			manager.Tell(new RequestFloorIds(1), probe.Ref);
		    var recieved = probe.ExpectMsg<RespondFloorIds>();

			Assert.Equal(1, recieved.RequestId);
			Assert.Empty(recieved.FloorIds);
	    }

	    [Fact]
	    public void RegisterNewFloorWhenDoesNotAlreadyExists()
	    {
		    var probe = CreateTestProbe();
		    var manager = Sys.ActorOf(FloorManagerActor.Props());

			manager.Tell(new RequestRegisterTemperatureSensor(1, "a", "1"), probe.Ref);
		    probe.ExpectMsg<RespondRegisterTemperatureSensor>(a => a.RequestId.Equals(1));

			manager.Tell(new RequestFloorIds(2), probe.Ref);
		    var recieved = probe.ExpectMsg<RespondFloorIds>();

			Assert.Equal(2, recieved.RequestId);
			Assert.Single(recieved.FloorIds);
			Assert.Contains("a", recieved.FloorIds);
	    }

	    [Fact]
	    public void ReuseExistingFloorWhenAlreadyExists()
	    {
		    var probe = CreateTestProbe();
		    var manager = Sys.ActorOf(FloorManagerActor.Props());

			manager.Tell(new RequestRegisterTemperatureSensor(1, "a", "1"), probe.Ref);
		    probe.ExpectMsg<RespondRegisterTemperatureSensor>(a => a.RequestId.Equals(1));

		    manager.Tell(new RequestRegisterTemperatureSensor(2, "a", "2"), probe.Ref);
		    probe.ExpectMsg<RespondRegisterTemperatureSensor>(a => a.RequestId.Equals(2));

			manager.Tell(new RequestFloorIds(3), probe.Ref);
		    var recieved = probe.ExpectMsg<RespondFloorIds>();

			Assert.Equal(3, recieved.RequestId);
		    Assert.Single(recieved.FloorIds);
			Assert.Contains("a", recieved.FloorIds);
	    }

	    [Fact]
	    public async Task ReturnFloorIdsOnlyFromActiveActors()
	    {
		    var probe = CreateTestProbe();
		    var manager = Sys.ActorOf(FloorManagerActor.Props(), "FloorManager");

			manager.Tell(new RequestRegisterTemperatureSensor(1, "a", "1"));
		    manager.Tell(new RequestRegisterTemperatureSensor(2, "b", "2"));

		    var firstFloor = await Sys.ActorSelection("akka://test/user/FloorManager/floor-a")
			    .ResolveOne(TimeSpan.FromSeconds(3));

		    probe.Watch(firstFloor);
			firstFloor.Tell(PoisonPill.Instance);
		    probe.ExpectTerminated(firstFloor);

			manager.Tell(new RequestFloorIds(3), probe.Ref);
		    var recieved = probe.ExpectMsg<RespondFloorIds>();

			Assert.Equal(3, recieved.RequestId);
		    Assert.Single(recieved.FloorIds);
		    Assert.Contains("b", recieved.FloorIds);
	    }

    }
}
