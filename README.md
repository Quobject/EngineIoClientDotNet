# EngineIoClientDotNet
====================

Engine.IO Client Library for .Net

This is the Engine.IO Client Library for C#, which is ported from the [JavaScript client](https://github.com/LearnBoost/engine.io-client).


## Installation


## Usage
EngineIoClientDotNet has a similar api to those of the JS client. 

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
		var dataString = (string)data;
		Console.WriteLine(dataString);
		socket.Close();
	});
});
socket.Open();            
```

## Features
This library supports all of the features the JS client does, including events, options and upgrading transport.

## License

MIT
