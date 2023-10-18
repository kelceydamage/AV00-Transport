# AV00 Transport
Inter-Service communication for AV00

Built on top of [NetMQ](https://netmq.readthedocs.io/en/latest/)

## Info
Details can be found here in the project plan readme
[AV00 Primary Repo](https://github.com/kelceydamage/AV00)

## Message Types
* Event
    * Any event we want to share with other services.
* EventReceipt
    * Allows tracking of event progress.

## Services
* Relay
** The Relay is a service that receives events and distributes them as necessary.

## Prebuilt Client Examples
* EventLogger
    * A special purpose client that is capable of sending a log event as well as writing it to the console.
* TransportClient
    * A barebones client that can push (and push async) events to the relay. This client can also receive events on various topics.

### Future Clients
* TBD but I'm sure something will come up. Especially when we start working with sensor data.

# Usage

## Setting Up A Client
In order to send and receive events, we need to configure a basic transport client. You can extend the basic transport client to add more functionality such as a socket for receiving event receipts. Currently the constructor for the basic transport client is going to need three arguments: `PushEventSocket`, `SubscribeEventSocket`, `TransportMessageFrameCount`.

These arguments can be passed from `ConfigurationManager`.
```c#
TransportClient myTransportClient = new(ConfigurationManager.ConnectionStrings, ConfigurationManager.AppSettings);
```

We can pass these arguments using our `IConfigurationService`.
```c#
TransportClient myTransportClient = new(Configuration)
```

Or we can manually pass the arguments.
```c#
TransportClient myTransportClient = new(SubscriberEventSocket, PushEventSocket, TransportMessageFrameCount)
```

## Using Events For Sharing Data With Other Services
The base object sent between services over ZeroMQ is an `Event<T>` (`IEvent`). Event is a simple carrier object capable of serializing and deserializing itself and an `EventModel` (`IEventModel`) it's carrying. You can see the documentation for `EventModel` here: [AV00 Shared Repo](https://github.com/kelceydamage/AV00-Shared)

In order to create an `Event<T>` for transport we must first create some data in the form of an `EventModel`. Once we have some data, we simply pass it to the `Event<T>` constructor to get an `Event`.
```c#
MotorCommandEventModel myData = new("DriveService",  EnumMotorCommands.Move, MotorDirection.Forwards, 1024)

Event<MotorCommandEventModel> @event = new(myData)
```

Assuming we have a transport client, we can send this event.
```c#
myTransportClient.PushEvent(@event);
```

We can then receive the event.
```c#
myTransportClient.RegisterServiceEventCallback("DriveService", MyCallbackFunction);
myTransportClient.ProcessPendingEvents()
```
