using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;
using BuildingMonitor.Messages.Medatata;
using BuildingMonitor.Messages.RegisterSensor;
using BuildingMonitor.Messages.UpdateTemperature;
using Xunit;

namespace BuildingMonitorTests
{
	public class TemperatureSensorActorShould : TestKit
    {
	    [Fact]
	    public void InitSensorMetaData()
	    {
		    var probe = CreateTestProbe();

		    var sensor = Sys.ActorOf(TemperatureSensorActor.Props("a", "1"));

			sensor.Tell(new RequestMetadata(1), probe.Ref);

		    var recieve = probe.ExpectMsg<RespondMetadata>();

			Assert.Equal(1, recieve.RequestId);
			Assert.Equal("a", recieve.FloorId);
		    Assert.Equal("1", recieve.SensorId);
		}

	    [Fact]
	    public void StartWithNotemperature()
	    {
		    var probe = CreateTestProbe();

		    var sensor = Sys.ActorOf(TemperatureSensorActor.Props("a", "1"));

			sensor.Tell(new RequestTemperature(1), probe.Ref);

		    var recieve = probe.ExpectMsg<RespondTemperature>();

			Assert.Null(recieve.Temperature);
	    }

	    [Fact]
	    public void ConfirmTemperatureUpdatd()
	    {
		    var probe = CreateTestProbe();

		    var sensor = Sys.ActorOf(TemperatureSensorActor.Props("a", "1"));

			sensor.Tell(new RequestUpdateTemperature(42, 100), probe.Ref);

		    probe.ExpectMsg<RespondTemperatureUpdated>(m => Assert.Equal(42, m.RequestId));
	    }

	    [Fact]
	    public void UpdateNewTemperature()
	    {
		    var probe = CreateTestProbe();

		    var sensor = Sys.ActorOf(TemperatureSensorActor.Props("a", "1"));

			sensor.Tell(new RequestUpdateTemperature(42, 50));
		    sensor.Tell(new RequestTemperature(1), probe.Ref);

		    var recieved = probe.ExpectMsg<RespondTemperature>();

			Assert.Equal(1, recieved.RequestId);
		    Assert.Equal(50, recieved.Temperature);
		}

	    [Fact]
	    public void RegisterNewSensor()
	    {
		    var probe = CreateTestProbe();

		    var sensor = Sys.ActorOf(TemperatureSensorActor.Props("a", "1"));

			sensor.Tell(new RequestRegisterTemperatureSensor(1, "a", "1"), probe.Ref);

		    var recieve = probe.ExpectMsg<RespondRegisterTemperatureSensor>();

			Assert.Equal(1, recieve.RequestId);
			Assert.Equal(sensor, recieve.SensorRef);
	    }

	    [Fact]
	    public void NotRegisterSensorWhenFloorIsWring()
	    {
		    var probe = CreateTestProbe();
		    var eventStreamProbe = CreateTestProbe();

		    Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

		    var sensor = Sys.ActorOf(TemperatureSensorActor.Props("a", "1"));

			sensor.Tell(new RequestRegisterTemperatureSensor(1, "b", "1"), probe.Ref);

			probe.ExpectNoMsg();

		    var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

		    Assert.IsType<RequestRegisterTemperatureSensor>(unhandled.Message);
	    }

	    [Fact]
	    public void NotRegisterSensorWhenSensorIdIsWring()
	    {
		    var probe = CreateTestProbe();
		    var eventStreamProbe = CreateTestProbe();

		    Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

		    var sensor = Sys.ActorOf(TemperatureSensorActor.Props("a", "1"));

		    sensor.Tell(new RequestRegisterTemperatureSensor(1, "a", "2"), probe.Ref);

		    probe.ExpectNoMsg();

		    var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

		    Assert.IsType<RequestRegisterTemperatureSensor>(unhandled.Message);
	    }
	}
}
