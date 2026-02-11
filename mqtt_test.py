import json
import time
import random
from datetime import datetime
import paho.mqtt.client as mqtt

# Configuration
BROKER = "broker.hivemq.com"
PORT = 1883
TENANT_ID = "3aa6163f-f889-48e9-a6fe-6d2ea42d57b4"
DEVICE_ID = "sensor-001"
TOPIC = f"sentinel/{TENANT_ID}/device/{DEVICE_ID}/telemetry"

def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print(f"Connected to MQTT broker: {BROKER}")
        print(f"Publishing to: {TOPIC}")
        print("-" * 60)
    else:
        print(f"Connection failed with code {rc}")

def on_publish(client, userdata, mid, properties=None, reason_code=None):
    print(f"Message published (mid: {mid})")

print("=" * 60)
print("MQTT Test Publisher for Sentinel.Events")
print("=" * 60)
print(f"Broker: {BROKER}:{PORT}")
print(f"Device: {DEVICE_ID}")
print("=" * 60)
print()

client = mqtt.Client(client_id="SentinelEvents_TestPublisher", protocol=mqtt.MQTTv5)
client.on_connect = on_connect
client.on_publish = on_publish

try:
    client.connect(BROKER, PORT, 60)
    client.loop_start()
    time.sleep(2)
except Exception as e:
    print(f"Failed to connect: {e}")
    exit(1)

print("Sending telemetry messages (Ctrl+C to stop)...")
print()

message_count = 0
try:
    while message_count < 5:  # Send 5 messages then stop
        temp = round(random.uniform(65, 95), 2)

        telemetry = {
            "type": "temperature",
            "data": {
                "temperature": temp,
                "humidity": round(random.uniform(30, 70), 2),
                "pressure": round(random.uniform(980, 1020), 2),
                "battery": round(random.uniform(70, 100), 2)
            },
            "timestamp": datetime.utcnow().isoformat() + "Z"
        }

        payload = json.dumps(telemetry)
        client.publish(TOPIC, payload, qos=1)
        message_count += 1

        print(f"[{datetime.now().strftime('%H:%M:%S')}] Message #{message_count}")
        print(f"   Temperature: {temp}F")
        if temp > 80:
            print(f"   ** Should trigger alert! **")
        print()

        time.sleep(3)

except KeyboardInterrupt:
    print()
finally:
    print(f"Sent {message_count} messages total.")
    client.loop_stop()
    client.disconnect()
