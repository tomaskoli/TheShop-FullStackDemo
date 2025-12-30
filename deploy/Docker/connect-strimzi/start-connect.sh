#!/bin/bash
set -e

# Kafka Connect startup script for Strimzi image

echo "=== Starting Kafka Connect ==="

# Build properties file from environment variables
CONNECT_PROPS="/tmp/connect-distributed.properties"

cat > $CONNECT_PROPS <<EOF
bootstrap.servers=${KAFKA_CONNECT_BOOTSTRAP_SERVERS:-kafka:9092}
group.id=${KAFKA_CONNECT_GROUP_ID:-connect-cluster}

config.storage.topic=${KAFKA_CONNECT_CONFIG_STORAGE_TOPIC:-_connect-configs}
config.storage.replication.factor=${KAFKA_CONNECT_CONFIG_STORAGE_REPLICATION_FACTOR:-1}

offset.storage.topic=${KAFKA_CONNECT_OFFSET_STORAGE_TOPIC:-_connect-offsets}
offset.storage.replication.factor=${KAFKA_CONNECT_OFFSET_STORAGE_REPLICATION_FACTOR:-1}

status.storage.topic=${KAFKA_CONNECT_STATUS_STORAGE_TOPIC:-_connect-status}
status.storage.replication.factor=${KAFKA_CONNECT_STATUS_STORAGE_REPLICATION_FACTOR:-1}

key.converter=${KAFKA_CONNECT_KEY_CONVERTER:-org.apache.kafka.connect.json.JsonConverter}
value.converter=${KAFKA_CONNECT_VALUE_CONVERTER:-org.apache.kafka.connect.json.JsonConverter}
key.converter.schemas.enable=${KAFKA_CONNECT_KEY_CONVERTER_SCHEMAS_ENABLE:-false}
value.converter.schemas.enable=${KAFKA_CONNECT_VALUE_CONVERTER_SCHEMAS_ENABLE:-false}

plugin.path=${KAFKA_CONNECT_PLUGIN_PATH:-/opt/kafka/plugins}

rest.port=${KAFKA_CONNECT_REST_PORT:-8083}
rest.advertised.host.name=${HOSTNAME:-localhost}

offset.flush.interval.ms=10000
EOF

echo "Configuration file:"
cat $CONNECT_PROPS

echo ""
echo "Plugins available:"
ls -la /opt/kafka/plugins/

echo ""
echo "Starting Kafka Connect..."
exec /opt/kafka/bin/connect-distributed.sh $CONNECT_PROPS

