"""
Automated 1-minute demo for Sentinel.Events - Multi-Device
Demonstrates real-time MQTT telemetry processing with multiple devices
"""

import paho.mqtt.client as mqtt
import json
import time
import requests
from datetime import datetime
import random

# Configuration
MQTT_BROKER = "broker.hivemq.com"
MQTT_PORT = 1883
TENANT_ID = "9E93AF42-20DF-45F0-9811-427191032C9E"
API_BASE_URL = "http://localhost:5262/api"
API_KEY = "test-api-key-12345"

# Multiple devices to test
DEVICES = [
    {"id": "sensor-001", "name": "Building A", "type": "temperature"},
    {"id": "sensor-002", "name": "Building B", "type": "temperature"},
    {"id": "sensor-003", "name": "Data Center", "type": "fire"},
    {"id": "sensor-004", "name": "Main Entrance", "type": "motion"}
]

# Colors for terminal output
class Colors:
    HEADER = '\033[95m'
    BLUE = '\033[94m'
    CYAN = '\033[96m'
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    RED = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'

def print_header(message):
    print(f"\n{Colors.BOLD}{Colors.HEADER}{'='*70}{Colors.ENDC}")
    print(f"{Colors.BOLD}{Colors.HEADER}{message.center(70)}{Colors.ENDC}")
    print(f"{Colors.BOLD}{Colors.HEADER}{'='*70}{Colors.ENDC}\n")

def print_info(label, value):
    print(f"{Colors.CYAN}{label}:{Colors.ENDC} {value}")

def print_success(message):
    print(f"{Colors.GREEN}[OK] {message}{Colors.ENDC}")

def print_warning(message):
    print(f"{Colors.YELLOW}[WARNING] {message}{Colors.ENDC}")

def print_error(message):
    print(f"{Colors.RED}[ERROR] {message}{Colors.ENDC}")

def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print_success(f"Connected to MQTT broker: {MQTT_BROKER}")
    else:
        print_error(f"Failed to connect to MQTT broker, return code: {rc}")

def send_telemetry(client, device_id, device_name, device_type):
    """Send a telemetry message via MQTT for a specific device"""
    # Generate temperature based on device type
    if device_type == "temperature":
        # Vary temperature - sometimes high to trigger alerts
        if random.random() > 0.6:
            temperature = round(random.uniform(82, 98), 1)  # High temp
        else:
            temperature = round(random.uniform(65, 78), 1)  # Normal temp
    else:
        temperature = round(random.uniform(68, 85), 1)  # Other devices

    topic = f"sentinel/{TENANT_ID}/device/{device_id}/telemetry"

    payload = {
        "type": "temperature",
        "data": {
            "temperature": temperature,
            "humidity": round(random.uniform(30, 70), 2),
            "pressure": round(random.uniform(980, 1020), 2),
            "battery": round(random.uniform(80, 100), 2)
        },
        "timestamp": datetime.utcnow().isoformat() + "Z"
    }

    result = client.publish(topic, json.dumps(payload), qos=1)

    temp_color = Colors.RED if temperature > 90 else Colors.YELLOW if temperature > 80 else Colors.GREEN
    print(f"  {Colors.CYAN}[{device_name}]{Colors.ENDC} {temp_color}{temperature}F{Colors.ENDC}")

    return result.rc == 0

def get_alerts():
    """Get current unacknowledged alerts from API"""
    try:
        headers = {"X-API-Key": API_KEY}
        response = requests.get(f"{API_BASE_URL}/alerts", headers=headers, timeout=5)

        if response.status_code == 200:
            data = response.json()
            if data.get("success"):
                return data.get("data", [])
        return []
    except Exception as e:
        print_error(f"Failed to get alerts: {e}")
        return []

def acknowledge_alert(alert_id):
    """Acknowledge an alert via API"""
    try:
        headers = {"X-API-Key": API_KEY}
        response = requests.post(
            f"{API_BASE_URL}/alerts/{alert_id}/acknowledge",
            headers=headers,
            timeout=5
        )

        if response.status_code == 200:
            return True
        return False
    except Exception as e:
        return False

def display_alerts(alerts):
    """Display current alerts"""
    if not alerts:
        print(f"  {Colors.GREEN}No active alerts{Colors.ENDC}")
        return

    print(f"\n  {Colors.BOLD}Active Alerts ({len(alerts)}):{Colors.ENDC}")
    for i, alert in enumerate(alerts, 1):
        severity = alert.get('severity')
        severity_color = Colors.RED if severity >= 4 else Colors.YELLOW if severity >= 3 else Colors.BLUE

        print(f"  {severity_color}{i}. {alert.get('deviceName')}: {alert.get('message')}{Colors.ENDC}")

def run_demo():
    """Run the automated multi-device demo for 1 minute"""
    print_header("SENTINEL.EVENTS - MULTI-DEVICE DEMO")
    print_info("Duration", "1 minute")
    print_info("MQTT Broker", f"{MQTT_BROKER}:{MQTT_PORT}")
    print_info("API Endpoint", API_BASE_URL)
    print_info("Devices", f"{len(DEVICES)} sensors")
    print()
    for device in DEVICES:
        print(f"  - {Colors.CYAN}{device['id']}{Colors.ENDC}: {device['name']} ({device['type']})")
    print()

    # Connect to MQTT
    client = mqtt.Client(client_id="SentinelEvents_MultiDeviceDemo", protocol=mqtt.MQTTv5)
    client.on_connect = on_connect

    try:
        client.connect(MQTT_BROKER, MQTT_PORT, 60)
        client.loop_start()
        time.sleep(2)  # Wait for connection

        start_time = time.time()
        iteration = 1

        print_header("DEMO STARTING")

        while time.time() - start_time < 60:  # Run for 1 minute
            elapsed = int(time.time() - start_time)
            remaining = 60 - elapsed

            print(f"\n{Colors.BOLD}{'-'*70}{Colors.ENDC}")
            print(f"{Colors.BOLD}[{datetime.now().strftime('%H:%M:%S')}] Iteration #{iteration} | Elapsed: {elapsed}s | Remaining: {remaining}s{Colors.ENDC}")
            print(f"{Colors.BOLD}{'-'*70}{Colors.ENDC}")

            # Send telemetry for all devices
            print(f"\n{Colors.BLUE}>> Sending telemetry from all devices...{Colors.ENDC}")
            for device in DEVICES:
                send_telemetry(client, device['id'], device['name'], device['type'])
                time.sleep(0.3)  # Small delay between devices

            # Wait for processing
            time.sleep(2)

            # Check for alerts
            print(f"\n{Colors.BLUE}>> Checking for alerts...{Colors.ENDC}")
            alerts = get_alerts()
            display_alerts(alerts)

            # Acknowledge alerts on even iterations
            if alerts and iteration % 2 == 0:
                print(f"\n{Colors.BLUE}>> Acknowledging {len(alerts)} alert(s)...{Colors.ENDC}")
                acknowledged = 0
                for alert in alerts:
                    if acknowledge_alert(alert.get('id')):
                        acknowledged += 1
                        print(f"  {Colors.GREEN}[OK]{Colors.ENDC} Acknowledged: {alert.get('deviceName')}")

                if acknowledged > 0:
                    time.sleep(1)
                    print(f"\n{Colors.BLUE}>> Updated alert list:{Colors.ENDC}")
                    updated_alerts = get_alerts()
                    display_alerts(updated_alerts)

            iteration += 1

            # Wait before next iteration
            if time.time() - start_time < 60:
                time.sleep(3)

        print_header("DEMO COMPLETED")
        print_success("Demo finished successfully!")
        print_info("Total Iterations", str(iteration - 1))

        # Final alert check
        print(f"\n{Colors.BOLD}Final Alert Status:{Colors.ENDC}")
        final_alerts = get_alerts()
        display_alerts(final_alerts)

    except KeyboardInterrupt:
        print_warning("\n\nDemo interrupted by user")
    except Exception as e:
        print_error(f"Demo error: {e}")
        import traceback
        traceback.print_exc()
    finally:
        client.loop_stop()
        client.disconnect()
        print(f"\n{Colors.GREEN}Disconnected from MQTT broker{Colors.ENDC}")

if __name__ == "__main__":
    run_demo()
