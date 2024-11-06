# How would you deploy, host and monitor the microservice?

One can containerized the application using docker and deploy them on Kubernetes cluster, which will manage raplicas, scale them based on the demand and can handle deployment strategies like rolling updates, ensuring minimal downtime during upgrades. To host them we can use EKS (AWS), AKS (Azure) or GKE (Google) or any other Kubernetes provider. We can used managed service where this provider will take care of infrastructure management, enhance scalability, security and provide integrations with logging and monitoring solutions like appinsight, cloudwatch, Grafana or ELK stack, Grafana, Azure monitor and alerts etc.

# How would you scale the service to handle an increase in traffic, ensuring that the rate-limiting and performance requirements are still met?

Use Kubernetes's horizontal pod scaling to automatically scale service pod based on CPU, date time, duration or custom metrics etc. 

# Set up infrastructure

Use docker compose file inside TapMangoSmsRateLimiter project
	To bring all container's up in deamon mode
		docker compose up -d
	To bring all container's down
		docker compose down

# Create a kafka topic

Using docker ps get kafka container id
	Create kafka topic using below command
	
		docker exec -it <kafka_container_name_or_id> kafka-topics --create --topic <kafka_topic_name> --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1

		Ex. docker exec -it c138ca35ba41 kafka-topics --create --topic sms_rate_limit --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1

	Verify Topic Creation

		docker exec -it <kafka_container_name_or_id> kafka-topics --list --bootstrap-server localhost:9092

		Ex. docker exec -it c138ca35ba41 kafka-topics --list --bootstrap-server localhost:9092

	Peek Message In Kafka Topic

		docker exec -it <kafka_container_name_or_id> kafka-console-consumer --bootstrap-server localhost:9092 --topic <kafka_topic_name> --from-beginning

		Ex. docker exec -it c138ca35ba41 kafka-console-consumer --bootstrap-server localhost:9092 --topic sms_rate_limit --from-beginning

# Cassandra Setup

	Access Cassandra container
		docker exec -it <cassandr_container_name_or_id> cqlsh

		Ex. docker exec -it cd0a013027d4 cqlsh

	Create key space

		CREATE KEYSPACE <keyspace_name> WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};

		Ex. CREATE KEYSPACE rate_limit WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};

	Create table for reporting

		CREATE TABLE rate_limit.sms_rate_limits (
			account_id int,
			phone_number bigint,
			datetime timestamp,
			can_send boolean,
			PRIMARY KEY ((account_id, phone_number), datetime)
		);
	Verify Data

		SELECT * FROM rate_limit.sms_rate_limits;