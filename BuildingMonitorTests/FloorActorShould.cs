using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;
using BuildingMonitor.Messages.RegisterSensor;
using BuildingMonitor.Messages.SensorIds;
using BuildingMonitor.Messages.TemperatureReading;
using BuildingMonitor.Messages.UpdateTemperature;
using Xunit;

namespace BuildingMonitorTests
{
    public class FloorActorShould : TestKit
    {

	    [Fact]
	    public void RegisterNewTemperatureSensorWhenDoesNotAlreadyExist()
	    {
		    var probe = CreateTestProbe();

		    var floor = Sys.ActorOf(FloorActor.Props("a"));

			floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);

		    var recieved = probe.ExpectMsg<RespondRegisterTemperatureSensor>();
			Assert.Equal(1, recieved.RequestId);

		    var sensorActor = probe.LastSender;
			sensorActor.Tell(new RequestUpdateTemperature(2, 200), probe.Ref);

		    probe.ExpectMsg<RespondTemperatureUpdated>();
	    }

	    [Fact]
	    public void ReturnExistingTemperatureSensorWhenReRegistringSameSensor()
	    {
		    var probe = CreateTestProbe();

		    var floor = Sys.ActorOf(FloorActor.Props("a"));

			floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "1"), probe.Ref);
			var received = probe.ExpectMsg<RespondRegisterTemperatureSensor>();
			Assert.Equal(1, received.RequestId);
		    var firstSensor = probe.LastSender;

		    floor.Tell(new RequestRegisterTemperatureSensor(2, "a", "1"), probe.Ref);
		    received = probe.ExpectMsg<RespondRegisterTemperatureSensor>();
		    Assert.Equal(2, received.RequestId);
		    var secondSensor = probe.LastSender;

			Assert.Equal(firstSensor, secondSensor);
		}

	    [Fact]
	    public void NotRegisterWhenMismatchedFloor()
	    {
		    var probe = CreateTestProbe();
		    var eventStreamProbe = CreateTestProbe();

		    Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));

		    var floor = Sys.ActorOf(FloorActor.Props("a"));

			floor.Tell(new RequestRegisterTemperatureSensor(1, "b", "1"), probe.Ref);
			probe.ExpectNoMsg();

		    var unhandled = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();
			Assert.IsType<RequestRegisterTemperatureSensor>(unhandled.Message);
			Assert.Equal(floor, unhandled.Recipient);
	    }

	    [Fact]
	    public void ReturnAllTemperatureSensorIds()
	    {
		    var probe = CreateTestProbe();
		    var floor = Sys.ActorOf(FloorActor.Props("a"));

			floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "1"), probe.Ref);
		    probe.ExpectMsg<RespondRegisterTemperatureSensor>();

		    floor.Tell(new RequestRegisterTemperatureSensor(2, "a", "2"), probe.Ref);
		    probe.ExpectMsg<RespondRegisterTemperatureSensor>();

			floor.Tell(new RequestTemperatureSensorIds(3), probe.Ref);
		    var response = probe.ExpectMsg<RespondTemperatureSensorIds>();

			Assert.Equal(2, response.SensorIds.Count);
			Assert.Contains("1", response.SensorIds);
		    Assert.Contains("2", response.SensorIds);
		}

	    [Fact]
	    public void ReturnEmptyListIfThereAreNosensors()
	    {
		    var probe = CreateTestProbe();
		    var floor = Sys.ActorOf(FloorActor.Props("a"));

			floor.Tell(new RequestTemperatureSensorIds(1), probe.Ref);
		    var response = probe.ExpectMsg<RespondTemperatureSensorIds>();

			Assert.Equal(0, response.SensorIds.Count);
	    }

	    [Fact]
	    public void ReturnTemperatureSensorIdsOnlyFromActiveActors()
	    {
		    var probe = CreateTestProbe();
		    var floor = Sys.ActorOf(FloorActor.Props("a"));

			floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "1"), probe.Ref);
		    probe.ExpectMsg<RespondRegisterTemperatureSensor>();
		    var firstSensorAdded = probe.LastSender;

		    floor.Tell(new RequestRegisterTemperatureSensor(2, "a", "2"), probe.Ref);
		    probe.ExpectMsg<RespondRegisterTemperatureSensor>();

			//stop actor
		    probe.Watch(firstSensorAdded);
			firstSensorAdded.Tell(PoisonPill.Instance);
		    probe.ExpectTerminated(firstSensorAdded);

			floor.Tell(new RequestTemperatureSensorIds(3), probe.Ref);
		    var response = probe.ExpectMsg<RespondTemperatureSensorIds>();

			Assert.Equal(1, response.SensorIds.Count);
		    Assert.Contains("2", response.SensorIds);

		}

	    [Fact]
	    public void ShouldInitiateQuery()
	    {
		    var probe = CreateTestProbe();
		    var floor = Sys.ActorOf(FloorActor.Props("a"));

		    floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);
		    probe.ExpectMsg<RespondRegisterTemperatureSensor>();
		    var sensor1 = probe.LastSender;

		    floor.Tell(new RequestRegisterTemperatureSensor(2, "a", "90"), probe.Ref);
		    probe.ExpectMsg<RespondRegisterTemperatureSensor>();
		    var sensor2 = probe.LastSender;

		    sensor1.Tell(new RequestUpdateTemperature(0, 50.4));
		    sensor2.Tell(new RequestUpdateTemperature(0, 100.8));

		    floor.Tell(new RequestAllTemperatures(1), probe.Ref);
		    var response = probe.ExpectMsg<RespondAllTemperatures>(x => x.RequestId == 1);

		    Assert.Equal(2, response.TemperatureReadings.Count);

		    var reading1 = Assert.IsType<TemperatureAvailable>(
			    response.TemperatureReadings["42"]);
		    Assert.Equal(50.4, reading1.Temperature);

		    var reading2 = Assert.IsType<TemperatureAvailable>(
			    response.TemperatureReadings["90"]);
		    Assert.Equal(100.8, reading2.Temperature);
	    }
	}
}
