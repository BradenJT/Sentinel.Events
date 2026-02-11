#!/usr/bin/env python3
"""
MQTT Test Publisher for Sentinel.Events
Sends telemetry messages to test the MQTT worker
"""

import json
import time
import random
from datetime import datetime

try:
    import paho.mqtt.client as mqtt
except ImportError:
    print("❌ paho-mqtt not installed. Install with: pip install paho-mqtt")
    exit(1)

# Configuration
BROKER = "broker.hivemq.com"
PORT = 1883
TENANT_ID = "3aa6163f-f889-48e9-a6fe-6d2ea42d57b4"  # From SeedData
DEVICE_ID = "sensor-001"  # One of your provisioned devices

# Topic format: sentinel/{tenantId}/device/{deviceId}/telemetry
TOPIC = f"sentinel/{TENANT_ID}/device/{DEVICE_ID}/telemetry"

def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print(f"✅ Connected to MQTT broker: {BROKER}")
        print(f"📡 Publishing to topic: {TOPIC}")
        print("-" * 60)
    else:
        print(f"❌ Connection failed with code {rc}")

def on_publish(client, userdata, mid, properties=None, reason_code=None):
    print(f"✓ Message published (mid: {mid})")

def generate_telemetry():
    """Generate realistic telemetry data"""
    return {
        "type": "temperature",
        "data": {
            "temperature": round(random.uniform(65, 95), 2),  # 65-95°F
            "humidity": round(random.uniform(30, 70), 2),
            "pressure": round(random.uniform(980, 1020), 2),
            "battery": round(random.uniform(70, 100), 2)
        },
        "timestamp": datetime.utcnow().isoformat() + "Z"
    }

def main():
    print("=" * 60)
    print("🚀 MQTT Test Publisher for Sentinel.Events")
    print("=" * 60)
    print(f"Broker: {BROKER}:{PORT}")
    print(f"Tenant ID: {TENANT_ID}")
    print(f"Device ID: {DEVICE_ID}")
    print("=" * 60)
    print()

    # Create MQTT client
    client = mqtt.Client(client_id="SentinelEvents_TestPublisher", protocol=mqtt.MQTTv5)
    client.on_connect = on_connect
    client.on_publish = on_publish

    # Connect to broker
    try:
        client.connect(BROKER, PORT, 60)
        client.loop_start()
        time.sleep(2)  # Wait for connection
    except Exception as e:
        print(f"❌ Failed to connect: {e}")
        return

    print("🔄 Sending telemetry messages (Ctrl+C to stop)...")
    print()

    message_count = 0
    try:
        while True:
            telemetry = generate_telemetry()
            payload = json.dumps(telemetry)

            result = client.publish(TOPIC, payload, qos=1)
            message_count += 1

            temp = telemetry["data"]["temperature"]
            emoji = "🚨" if temp > 80 else "✓"
            print(f"{emoji} [{datetime.now().strftime('%H:%M:%S')}] Message #{message_count}")
            print(f"   Temperature: {temp}°F")
            print(f"   Payload: {payload}")

            if temp > 80:
                print(f"   ⚠️  This should trigger an alert!")

            print()

            time.sleep(5)  # Send every 5 seconds

    except KeyboardInterrupt:
        print()
        print("=" * 60)
        print(f"✅ Stopped. Sent {message_count} messages total.")
        print("=" * 60)
    finally:
        client.loop_stop()
        client.disconnect()

if __name__ == "__main__":
    main()
