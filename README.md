# EngineIoClientDotNet

Engine.IO Client Library for .Net

Fork from [https://github.com/joewen/EngineIoClientDotNet](https://github.com/joewen/EngineIoClientDotNet).

This is the Engine.IO Client Library for C#, which is ported from the [JavaScript client](https://github.com/Automattic/engine.io-client).


##### Installation
Nuget install:
```
Install-Package EngineIoClientDotNet.Standard
```

* NuGet Package: [![EngineIoClientDotNet.Standard](https://img.shields.io/nuget/v/EngineIoClientDotNet.Standard.svg?maxAge=2592000)](https://www.nuget.org/packages/EngineIoClientDotNet.Standard/)


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

#### Framework Versions
.Net Standard 2.0

## License

[MIT](http://opensource.org/licenses/MIT)

