# Simplified Dockerfile using base Kafka Connect image with minimal dependencies
FROM quay.io/strimzi/kafka:0.38.0-kafka-3.6.0

USER root

# Install basic tools
RUN microdnf install -y curl && microdnf clean all

# Create plugin directory
RUN mkdir -p /opt/kafka/plugins && chmod 755 /opt/kafka/plugins

# Create directories for each connector
RUN mkdir -p /opt/kafka/plugins/jdbc-connector/lib && \
    mkdir -p /opt/kafka/plugins/neo4j-connector

# Download only essential JARs with simpler approach
WORKDIR /opt/kafka/plugins

# PostgreSQL JDBC driver (most reliable download)
RUN curl -sSL -o jdbc-connector/lib/postgresql-42.7.4.jar \
    "https://jdbc.postgresql.org/download/postgresql-42.7.4.jar" || \
    echo "Warning: PostgreSQL driver download failed"

# Confluent JDBC Connector (download main connector JAR)
RUN curl -sSL -f -o jdbc-connector/kafka-connect-jdbc-10.7.4.jar \
      "https://packages.confluent.io/maven/io/confluent/kafka-connect-jdbc/10.7.4/kafka-connect-jdbc-10.7.4.jar" \
    || curl -sSL -f -o jdbc-connector/kafka-connect-jdbc-10.7.4.jar \
      "https://repo1.maven.org/maven2/io/confluent/kafka-connect-jdbc/10.7.4/kafka-connect-jdbc-10.7.4.jar" \
    || echo "Warning: JDBC connector JAR download failed"

# Neo4j connector (from GitHub releases)  
RUN curl -sSL -o neo4j-connector/neo4j-kafka-connect-5.1.19.jar \
    "https://github.com/neo4j/neo4j-kafka-connector/releases/download/5.1.19/neo4j-kafka-connect-5.1.19.jar" || \
    echo "Warning: Neo4j connector download failed"

# Notes:
# - JDBC connector requires the main Confluent JAR and a DB driver (PostgreSQL provided above)
# - Neo4j connector is a single self-contained JAR

# Set ownership
RUN chown -R 1001:1001 /opt/kafka/plugins

USER 1001

# Environment variables
ENV KAFKA_CONNECT_PLUGIN_PATH=/opt/kafka/plugins

# Copy startup script
COPY --chown=1001:1001 start-connect.sh /opt/kafka/start-connect.sh
RUN chmod +x /opt/kafka/start-connect.sh

EXPOSE 8083

CMD ["/opt/kafka/start-connect.sh"]
