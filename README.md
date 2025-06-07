[![CI](https://github.com/Abaddax/Modbus/actions/workflows/ci.yml/badge.svg)](https://github.com/Abaddax/Modbus/actions/workflows/ci.yml)

# Modbus TCP/RTU Client/Server 

A basic implementation of Modbus TCP/RTU client/server library written in C#.

# Supported Function Codes

* Read Coils (FC1)
* Read Discrete Inputs (FC2)
* Read Holding Registers (FC3)
* Read Input Registers (FC4)
* Write Single Coil (FC5)
* Write Single Register (FC5)
* Write Multiple Coils (FC16)
* Write Multiple Registers (FC16) 

# Usage

## Modbus TCP

### Client

```csharp
using var modbusClient = new ModbusTcpClientBuilder()
    .WithUnitIdentifier(1)
    .WithServer("127.0.0.1", 502)
    .Build();

await modbusClient.ConnectAsync();

var coil = await modbusClient.ReadCoilsAsync(0x01, 1);

var inputRegisters = await modbusClient.ReadInputRegistersAsync(0x01, 2);

var inputRegistersAsFloat = inputRegisters.GetRegisterBytes().ReadAsFloat().First();

```

### Server

```csharp
using var modbusServer = new ModbusTcpServerBuilder()
    .WithUnitIdentifier(1)
    .WithMaxServerConnections(1)
    .WithEndpoint(IPAddress.Loopback, 502)
    .WithServerData(/*Server-Data-Implementation*/)
    .Build();

await modbusServer.StartAsync();

//Do something

await modbusServer.StopAsync();
```

## Modbus RTU

### Client

```csharp
using var modbusClient = new ModbusRtuClientBuilder()
    .WithUnitIdentifier(1)
    .WithPortName("COM1")
    .WithBaudRate(9600)
    .WithParity(Parity.None)
    .WithStopBits(StopBits.None)
    .Build();

await modbusClient.ConnectAsync();

var coil = await modbusClient.ReadCoilsAsync(0x01, 1);

var inputRegisters = await modbusClient.ReadInputRegistersAsync(0x01, 2);

var inputRegistersAsFloat = inputRegisters.GetRegisterBytes().ReadAsFloat().First();

```

### Server

```csharp
using var modbusServer = new ModbusRtuServerBuilder()
    .WithUnitIdentifier(1)
    .WithPortName("COM1")
    .WithBaudRate(9600)
    .WithParity(Parity.None)
    .WithStopBits(StopBits.None)
    .WithServerData(/*Server-Data-Implementation*/)
    .Build();

await modbusServer.StartAsync();

//Do something

await modbusServer.StopAsync();
```