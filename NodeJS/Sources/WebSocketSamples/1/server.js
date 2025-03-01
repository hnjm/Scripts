const express = require('express');
const http = require('http');
const webSocket = require('ws');

const app = express();
const httpServer = http.createServer(app);

const PORT = process.env.PORT || 3000;

const wsServer = new webSocket.Server({
    server: httpServer
}, () => console.log(`WS server is listening at ws://localhost:${WS_PORT}`)); // listen to WS (web-sockets) [ws://]

let connectedClients = [];
wsServer.on('connection', (webSocket, request) => {
    console.log('socket Connected');
    
    connectedClients.push(webSocket);    
    webSocket.on('message', data => {        
        connectedClients.forEach((ws, i) => {
            if (ws.readyState === ws.OPEN) ws.send(data); // send data through socket
            else connectedClients.splice(i, 1);  // closed socket - remove from collection
        });
    });
});

app.get('/', (request, response) => {
    response.send('<p>WebSocket SAMPLE</p>');
});
httpServer.listen(PORT, () => console.log(`HTTP server listening at http://localhost:${PORT}`)); // listen to HTTP [http://]
