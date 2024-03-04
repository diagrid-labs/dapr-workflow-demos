import logging
import os

from flask import Flask, jsonify, render_template, request
from flask_socketio import SocketIO

APP_PORT = os.getenv("APP_PORT", "3001")
TOPIC_NAME = os.getenv("TOPIC_NAME", "notifications")

app = Flask(__name__)
socketio = SocketIO(app)

@socketio.on('connect')
def socket_connect():
    print('connected', flush=True)

@app.route('/')
def index():
    return render_template('index.html')


@app.route('/' + TOPIC_NAME, methods=['POST', 'PUT'])
def topic_notifications():
    logging.info(f"Received notification: {request.json}")
    event = request.json
    socketio.emit('message', event)
    return {
        "success": True,
    }, 200


@app.route("/healthz", methods=["GET"])
def hello():
    return f"Hello from {__name__}", 200


def main():
    logging.info("Starting Flask app...")
    socketio.run(app, port=APP_PORT, allow_unsafe_werkzeug=True)


if __name__ == "__main__":
    logging.basicConfig(
        format='%(asctime)s.%(msecs)03d %(levelname)s: %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S',
        level=logging.INFO)
    main()
