var
ssl = true,
express = require('express'),
fs = require('fs'),
strip_json = require('strip-json-comments'),
config = JSON.parse(strip_json(String(fs.readFileSync('./../grunt/config.json')))),
util =  require('util'),
app = express(),
fs = require('fs'),
options = {
  key: fs.readFileSync(__dirname + '/testme.quobject.com.key'),
  cert: fs.readFileSync(__dirname + '/testme.quobject.com.cert'),
  requestCert: true
},
server,
https,
http,
primus_server,
ssl_server ;


console.log("https port = " + config.server.ssl_port);
https = require('https').createServer(options, app);
ssl_server = require('engine.io').attach(https, {'pingInterval': 500});
https.listen(config.server.ssl_port, function(d) {
  console.log('Engine.IO server listening on port', config.server.ssl_port);
});

console.log("http port = " + config.server.port);
http = require('http').createServer(app);
server = require('engine.io').attach(http, { 'pingInterval': 500 });
primus_server = require('engine.io').attach(http, { 'pingInterval': 500, 'path' : '/primus/engine.io' });
http.listen( config.server.port, function() {
  console.log('Engine.IO server listening on port', config.server.port);
});


primus_server.on('connection', function (socket) {
  console.log('primus_server connection');
  socket.send('hi');
});


http.on('request', function(request, response) {
  //console.log('request ' +util.inspect( request.headers));
 
});




server.on('connection', function(socket){
  socket.send('hi');




  // Bounce any received messages back
  socket.on('message', function (data) {

    console.log('got message1 data = "' + data + '"');
    console.log('got message data stringify = "' + JSON.stringify(data) + '"');
    var result = new Int8Array(data);
    console.log('got message data Int8Array = "' + JSON.stringify(result) + '"\n\n');

    if (data === 'give binary') {
      var abv = new Int8Array(5);
      for (var i = 0; i < 5; i++) {
        abv[i] = i;
      }
      socket.send(abv);
      return;
    }

    if (data === 'cookie') {
      console.log('cookie ' + util.inspect(socket.request.headers));
      if (socket.request.headers !== undefined) {
        if (socket.request.headers.cookie === "foo=bar") {
          socket.send('got cookie');
          return;
        }
      }
      socket.send('no cookie');
      return;
    }


    socket.send(data);

  });

});




ssl_server.on('connection', function(socket){
  socket.send('hi');

  // Bounce any received messages back
  socket.on('message', function (data) {
    if (data === 'give binary') {
      var abv = new Int8Array(5);
      for (var i = 0; i < 5; i++) {
        abv[i] = i;
      }
      socket.send(abv);
      return;
    }
    console.log('got message data = "' + data + '"');
    console.log('got message data stringify = "' + JSON.stringify(data) + '"');
    var result = new Int8Array(data);
    console.log('got message data Int8Array = "' + JSON.stringify(result) + '"\n\n');

    socket.send(data);

  });
});