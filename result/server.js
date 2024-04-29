const express = require('express');  // Framework para crear servidores web
const http = require('http');  // Módulo HTTP de Node.js
const socketIO = require('socket.io');  // Para gestionar WebSockets
const { Client } = require('pg');  // Cliente para PostgreSQL

// Crear el servidor Express
const app = express();
const server = http.createServer(app);
const io = socketIO(server);  // Integrar Socket.io con el servidor HTTP

// Conexión a PostgreSQL
const pgHost = process.env.PG_HOST || 'localhost';
const pgPort = parseInt(process.env.PG_PORT || '5432', 10);
const pgUser = process.env.PG_USER || 'postgres';
const pgPassword = process.env.PG_PASSWORD || 'password';
const pgDatabase = process.env.PG_DATABASE || 'voting';

// Conexión de PostgreSQL
const pgClient = new Client({
  host: pgHost,
  port: pgPort,
  user: pgUser,
  password: pgPassword,
  database: pgDatabase,
});

pgClient.connect();  // Conectar a PostgreSQL

// Cargar la página inicial para mostrar resultados
app.get('/', (req, res) => {
  res.sendFile(__dirname + '/index.html');  // Enviar la página HTML
});

// Cuando un cliente se conecta, enviarle los resultados actuales
io.on('connection', async (socket) => {
  console.log('Cliente conectado');

  // Obtener los resultados actuales de la base de datos
  const result = await pgClient.query('SELECT * FROM voting_results');

  // Enviar los resultados al cliente
  socket.emit('results', result.rows);

  // Desconexión de un cliente
  socket.on('disconnect', () => {
    console.log('Cliente desconectado');
  });
});

// Iniciar el servidor
const PORT = process.env.PORT || 4000;
server.listen(PORT, () => {
  console.log(`Servidor escuchando en el puerto ${PORT}`);
});
