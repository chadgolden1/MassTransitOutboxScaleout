# MassTransitOutboxScaleout
Trying out some scenarios with MassTransit's Transactional Outbox.

This sample is inspired heavily by the MassTransit [sample transactional outbox application](https://github.com/MassTransit/Sample-Outbox).

# How to run (Docker)
```
docker-compose up --build
```

This will spin up a SQL Server, RabbitMQ, create the database, and run the entire sample.

# Architecture
This simple application uses the **Producer** to write messages to the outbox. A separate **Sweeper** service polls the messages written to the outbox table, and delivers them to the broker (RabbitMQ). The **Consumer** simply consumes the messages.

Projects:
- Producer: Publishes a handful of notify-registration messages every second to the transactional outbox
- Sweeper: Polls the outbox table and delivers messages from the transactional outbox to the broker
- Consumer: Consumes notify-registration messages
- CreateDb: Ensures a fresh copy of the Registration database is created prior to running the other services