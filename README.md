# THIS PROJECT IS DEPRECATED
Component is not maintained anymore. See https://github.com/Quobject/EngineIoClientDotNet/issues/69 for more information.

#### EngineIoClientDotNet
====================

Engine.IO Client Library for .Net

This is the Engine.IO Client Library for C#, which is ported from the [JavaScript client](https://github.com/Automattic/engine.io-client).


##### Installation
Nuget install:
```
Install-Package EngineIoClientDotNet
```

* NuGet Package: [![EngineIoClientDotNet](https://img.shields.io/nuget/v/EngineIoClientDotNet.svg?maxAge=2592000)](https://www.nuget.org/packages/EngineIoClientDotNet/)


##### Usage
EngineIoClientDotNet has a similar api to those of the [JavaScript client](https://github.com/Automattic/engine.io-client).

You can use `Socket` to connect:

```cs
var socket = new Socket("ws://localhost");
socket.On(Socket.EVENT_OPEN, () =>
{
	socket.Send("hi", () =>
	{		
		socket.Close();
	});
});
socket.Open();
```

Receiving data
```cs
var socket = new Socket("ws://localhost");
socket.On(Socket.EVENT_OPEN, () =>
{
	socket.On(Socket.EVENT_MESSAGE, (data) =>
	{
		Console.WriteLine((string)data);
	});
});
socket.Open();            
```

#### Features
This library supports all of the features the JS client does, including events, options and upgrading transport.



## Framework Versions
.NETFramework v3.5, .NETFramework v4.0, .NETFramework v4.5


## License

[MIT](http://opensource.org/licenses/MIT)

