# app.py
from flask import Flask, request, render_template
import redis
import os

app = Flask(__name__)
redis_host = os.getenv('REDIS_HOST', 'localhost')  # Dirección del servicio Redis
redis_port = int(os.getenv('REDIS_PORT', 6379))  # Puerto del servicio Redis

redis_conn = redis.StrictRedis(host=redis_host, port=redis_port, decode_responses=True)

@app.route('/')
def home():
    return render_template('index.html')  # Retorna la interfaz de votación

@app.route('/vote', methods=['POST'])
def vote():
    option = request.form['option']
    redis_conn.incr(option)  # Incrementa el contador del voto
    return 'Voto registrado'

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
