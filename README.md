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
* TaskExecutorClient

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
