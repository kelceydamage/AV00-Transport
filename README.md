# AV00-Transport
Inter-Service communication for AV00

Built on top of [NetMQ](https://netmq.readthedocs.io/en/latest/)

## Message Types
* TaskEvent
    * Task control the robot.
* TaskEventReceipt
    * Allows tracking of task progress

### Future Messages
* SensorEvent
    * Stream of compiled sensor data from a sensor arrays controller. Not raw sensor data.

## Services
* Relay (Might break into SensorRelay and TaskRelay)

## Prebuilt Client
* ServiceBusClient
    * Any service that can issue tasks/control the physical robot.
* TaskExecutorClient
    * Any services that directly manage hardware and can execute tasks.

### Future Clients
* TBD but I'm sure something will come up. Especially when we start working with sensor data.

## System Overview
ServiceBusClient 
                     --TaskEvent-> 
                                        Relay 
                                                  --TaskEvent->  
                                                                      TaskExecutorClient 
                                                <-TaskEventReceipt-- 
                                        Relay      
                  <-TaskEventReceipt-- 
ServiceBusClient
