# How would you deploy, host and monitor the microservice?

One can containerized the application using docker and deploy them on Kubernetes cluster, which will manage raplicas, scale them based on the demand and can handle deployment strategies like rolling updates, ensuring minimal downtime during upgrades. To host them we can use EKS (AWS), AKS (Azure) or GKE (Google) or any other Kubernetes provider. We can used managed service where this provider will take care of infrastructure management, enhance scalability, security and provide integrations with logging and monitoring solutions like appinsight, cloudwatch, Grafana or ELK stack, Grafana, Azure monitor and alerts etc.

# How would you scale the service to handle an increase in traffic, ensuring that the rate-limiting and performance requirements are still met?

Use Kubernetes's horizontal pod scaling to automatically scale service pod based on CPU, date time, duration or custom metrics etc. 