# 📡 MQTT Real-Time Telemetry Testing

This guide shows you how to test the real-time MQTT telemetry processing in Sentinel.Events.

## 🎯 Overview

The system uses **MQTT** (Message Queuing Telemetry Transport) to receive real-time telemetry from IoT devices. The MqttTelemetryWorker connects to a broker and processes incoming messages.

## 🔧 Broker Options

### Option 1: HiveMQ Public Broker (Cloud - Easiest)
**Already configured** in `appsettings.json`:
- **Host**: `broker.hivemq.com`
- **Port**: `1883`
- **No authentication required**
- ✅ Works out of the box

### Option 2: Eclipse Mosquitto (Local)
Install locally for more control:

**Windows:**
```bash
# Download from: https://mosquitto.org/download/
# Or use Chocolatey:
choco install mosquitto

# Start broker:
mosquitto -v
```

**Update `appsettings.json`:**
```json
"Mqtt": {
  "BrokerHost": "localhost",
  "BrokerPort": 1883,
  ...
}
```

### Option 3: EMQX (Docker)
```bash
docker run -d --name emqx -p 1883:1883 -p 8083:8083 -p 8084:8084 -p 8883:8883 -p 18083:18083 emqx/emqx:latest
```

## 🚀 Quick Start

### Step 1: Start the API
```bash
cd Sentinel.Api
dotnet run
```

Watch for the MQTT connection logs:
```
✅ Connected to MQTT broker at broker.hivemq.com:1883
📡 Subscribed to topic: sentinel/+/device/+/telemetry
💡 Send test messages to: sentinel/{tenantId}/device/{deviceId}/telemetry
```

### Step 2: Send Test Messages

#### Option A: Python Publisher (Recommended)
```bash
# Install dependency
pip install paho-mqtt

# Run publisher
python mqtt_publisher.py
```

#### Option B: .NET Script Publisher
```bash
# Install dotnet-script globally (first time only)
dotnet tool install -g dotnet-script

# Run publisher
dotnet script mqtt_publisher.csx
```

#### Option C: MQTT Explorer (GUI Tool)
1. Download [MQTT Explorer](http://mqtt-explorer.com/)
2. Connect to `broker.hivemq.com:1883`
3. Publish to topic: `sentinel/3aa6163f-f889-48e9-a6fe-6d2ea42d57b4/device/sensor-001/telemetry`
4. Payload:
```json
{
  "type": "temperature",
  "data": {
    "temperature": 85,
    "humidity": 45,
    "pressure": 1013,
    "battery": 95
  },
  "timestamp": "2026-02-11T00:00:00Z"
}
```

#### Option D: mosquitto_pub Command Line
```bash
mosquitto_pub -h broker.hivemq.com -t "sentinel/3aa6163f-f889-48e9-a6fe-6d2ea42d57b4/device/sensor-001/telemetry" -m '{"type":"temperature","data":{"temperature":85},"timestamp":"2026-02-11T00:00:00Z"}'
```

## 📋 What to Watch For

### In the API Console:
```
📨 Received message on sentinel/{tenant}/device/{device}/telemetry: {...}
✅ Processed telemetry for device sensor-001 (Temperature Sensor - Building A)
🚨 Generated temperature alert for device sensor-001: 85.0°F
```

### In the Database:
Check `TelemetryEvents` table:
```sql
SELECT TOP 10 * FROM TelemetryEvents ORDER BY ReceivedAt DESC
```

Check for auto-generated alerts:
```sql
SELECT TOP 10 * FROM Alerts ORDER BY CreatedAt DESC
```

### Via API:
```bash
# Get recent alerts (should show auto-generated alerts from telemetry)
curl -s http://localhost:5262/api/alerts -H "X-API-Key: test-api-key-12345"
```

## 🎯 Topic Structure

```
sentinel/{tenantId}/device/{deviceId}/telemetry
```

- **tenantId**: Your tenant's GUID (e.g., `3aa6163f-f889-48e9-a6fe-6d2ea42d57b4`)
- **deviceId**: Device string ID (e.g., `sensor-001`, NOT the GUID)

## 📦 Message Format

```json
{
  "type": "temperature",
  "data": {
    "temperature": 85.5,
    "humidity": 45.2,
    "pressure": 1013.25,
    "battery": 95.8
  },
  "timestamp": "2026-02-11T03:00:00Z"
}
```

### Alert Triggers:
- **Temperature > 80°F**: High severity alert
- **Temperature > 90°F**: Critical severity alert

## 🔍 Troubleshooting

### Worker falls back to simulation mode
**Symptom:** See "Running in SIMULATION MODE" in logs

**Causes:**
- Broker is unreachable
- Network/firewall blocking port 1883
- Invalid broker configuration

**Solution:** Check `appsettings.json` and ensure broker is accessible

### Messages not received
**Check:**
1. Correct topic format (5 parts: `sentinel/{tenant}/device/{device}/telemetry`)
2. TenantId matches your database tenant
3. DeviceId exists and is provisioned
4. Broker connection is active

### Alerts not generating
**Check:**
1. Temperature in payload is > 80
2. No duplicate alert in last 5 minutes
3. Device exists in database
4. Check SQL Server for `Alerts` table

## 📊 Monitoring Tips

### View Live Telemetry in Database:
```sql
-- Real-time telemetry events
SELECT
    t.ReceivedAt,
    d.DeviceId,
    d.Name AS DeviceName,
    t.EventType,
    t.Payload
FROM TelemetryEvents t
JOIN Devices d ON t.DeviceId = d.Id
ORDER BY t.ReceivedAt DESC
```

### Check Alert Generation:
```sql
-- Recent auto-generated alerts
SELECT
    a.CreatedAt,
    d.DeviceId,
    d.Name AS DeviceName,
    a.Type,
    a.Severity,
    a.Message,
    a.IsAcknowledged
FROM Alerts a
JOIN Devices d ON a.DeviceId = d.Id
WHERE a.CreatedAt > DATEADD(HOUR, -1, GETUTCDATE())
ORDER BY a.CreatedAt DESC
```

## 🎮 Demo Scenarios

### Scenario 1: Normal Operation
Send temperature = 75°F → No alert, just telemetry stored

### Scenario 2: High Temperature
Send temperature = 85°F → High severity alert generated

### Scenario 3: Critical Temperature
Send temperature = 95°F → Critical severity alert generated

### Scenario 4: Deduplication
Send temperature = 95°F twice within 5 minutes → Only 1 alert created

## 🌐 Public Brokers (No Setup Required)

All these work out of the box:
- `broker.hivemq.com:1883` ✅ (Already configured)
- `test.mosquitto.org:1883`
- `broker.emqx.io:1883`

Just update `appsettings.json` and restart the API!

## 🔐 Security Notes

For production:
- Use TLS/SSL (port 8883)
- Add username/password authentication
- Use private broker (not public)
- Implement API key validation in messages

## 📈 Next Steps

1. **Add more alert rules** in `CheckAlertConditionsAsync()`
2. **Custom telemetry types** (motion, door, smoke, etc.)
3. **Device commands** (publish back to devices)
4. **Dashboard integration** (SignalR for real-time UI updates)
5. **Grafana visualization** (query telemetry data)
