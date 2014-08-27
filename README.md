# EngineIoClientDotNet
====================

Engine.IO Client Library for .Net

This is the Engine.IO Client Library for C#, which is ported from the [JavaScript client](https://github.com/LearnBoost/engine.io-client).


## Installation


## Usage
EngineIoClientDotNet has a similar api to those of the JS client. You can use `Socket` to connect:

```cs
var options = new Socket.Options();
options.Port = 3000;
options.Hostname = "localhost";

socket = new Socket(options);
socket.On(Socket.EVENT_OPEN, () =>
{	
	socket.Send("hi");
	socket.Close();
});
socket.Open();
```
            


## Features


## License

MIT
