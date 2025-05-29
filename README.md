# Modbus TCP Client/Server 

A basic implementation of Modbus TCP client/server library written fully in C#.

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

## Client

```
using var modbusClient = new ModbusTcpClientBuilder()
	.WithUnitIdentifier(1)
    .WithServer("127.0.0.1", 502)
    .Build();

await modbusClient.ConnectAsync();

var coil = await modbusClient.ReadCoilsAsync(0x01, 1);

var inputRegisters = await modbusClient.ReadInputRegistersAsync(0x01, 2);

var inputRegistersAsFloat = inputRegisters.GetRegisterBytes().ReadAsFloat().First();

```

## Server

```
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

# Future features

Support for Modbus RTU and other FCs might get added later...