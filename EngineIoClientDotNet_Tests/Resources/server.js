var
  ssl = true,
  express = require('express'),
  app = express(),
  fs = require('fs'),
  options = {
    key: fs.readFileSync(__dirname + '/testme.quobject.com.key'),
    cert: fs.readFileSync(__dirname + '/testme.quobject.com.cert'),
    requestCert: true
  },
  server,
  https,
  http;


//if (ssl === true) {

  console.log("https");
  https = require('https').createServer(options, app);
  ssl_server = require('engine.io').attach(https, {'pingInterval': 500});
  https.listen(443, function(d) {
    console.log('Engine.IO server listening on port', 443);
  });
//} else {

  console.log("http");
  http = require('http').createServer(app);
  server = require('engine.io').attach(http, {'pingInterval': 500});
  http.listen(80, function() {
    console.log('Engine.IO server listening on port', 80);
  });
//}



server.on('connection', function(socket){
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