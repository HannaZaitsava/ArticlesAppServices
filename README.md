### ArticlesApp

**ArticlesApp** is an Event-Driven application built on the **.NET (C#)** platform for publishing articles and asynchronously notifying authors. 

### Project Purpose

This project was developed as a hands-on learning initiative to master **Microservice Architecture** and gain practical, deep expertise in designing **asynchronous inter-service communication using Apache Kafka**. It showcases how to transition from tight service coupling to a resilient distributed system. 

### Tech Stack

* **Platform:** .NET / ASP.NET Core (C#)
* **Message Broker:** Apache Kafka
* **Primary Database:** PostgreSQL (stores business data and the Outbox table)
* **Caching & Inbox:** Redis (utilizes the new .NET HybridCache and Inbox store)
* **Logging:** Seq (structured logging)
* **Administration Tools:** UI for Apache Kafka, pgAdmin, RedisInsight

### Architecture & Resilience Patterns

The project is designed with a strong focus on resilience and data consistency across distributed systems: 

### 1. Transactional Outbox (Publisher)

* When an article is created in the **Articles Service**, the article entity and its corresponding article-published integration event are saved to **PostgreSQL** within a single local database transaction. This effectively solves the Dual Write problem.
* A dedicated background worker (articles-outbox-worker) periodically polls unsent events from the database and publishes them to Kafka.
* **Retry Policy:** The producer is configured with robust retry mechanics to handle transient network hiccups or brief broker unavailability.

### 2. Idempotent Consumer & DLQ (Subscriber)

* The **Notification Service** subscribes to the article-published topic, which is split into 3 partitions to support parallel processing.
* **Inbox Pattern (Redis):** Before executing any business logic, the service checks the incoming message's unique identifier against Redis. If the event has already been processed, it is safely skipped, preventing duplicate notifications (Idempotency).
* **Error Handling:** 

  * A strict retry policy is enforced for failed message consumption attempts.
  * If a message continuously fails after a specified number of retries, it is automatically routed to the **Dead Letter Queue (DLQ)** topic (article-published.dlq) for isolation and manual troubleshooting.

### Interaction Architecture (Data Flow)

1. **User** ➔ Creates an article ➔ **Articles API**.
2. **Articles API** ➔ Saves the `Article` to the articles table and the `Event` to the `Outbox` table within a **single database transaction**.
3. **Outbox Worker** ➔ Polls new records from the `Outbox` table ➔ Publishes the `ArticlePublishedEvent` to **Kafka** ➔ Marks the event as sent in the database.
4. **Notification Service** ➔ Consumes the event from Kafka ➔ Validates the `ArticleId` against **Redis (Inbox)**:
   * **If the event has already been processed:** Safely skips the duplicate message.
   * **If the event is new:** Saves the key to Redis and sends a notification to the author.
   * **If the message is a "Poison Pill" (invalid format or persistent processing error):** The service catches the exception after failed deserialization and automatically routes the corrupted message to the **Dead Letter Queue (DLQ)** topic for isolation and manual analysis.

### Apache Kafka Topics

The current cluster topology includes the following topics: 

* **article-published** (3 partitions) — The primary channel streaming integration events for newly published articles.
* **article-published.dlq** (1 partition) — The Dead Letter Queue holding invalid or unprocessable messages.

### Quick Start

1. Clone the repository: 

```bash
git clone https://github.com/HannaZaitsava/ArticlesAppServices.git
cd ArticlesApp
```
2. Spin up the entire infrastructure and services with a single command: 

```bash
docker-compose up -d
```
3. Open the management consoles in your browser to monitor the system: 

  * **Kafka UI:** http://localhost:8080
  * **Seq Logs:** http://localhost:5341
  * **pgAdmin:** http://localhost:5051

## 🔑 Authentication & Swagger Guide

At this stage, the project utilizes the built-in **ASP.NET Core Identity** system. To interact with protected endpoints (e.g., creating, editing, deleting or publishing articles), you can authenticate directly through the **Swagger UI**:

### Step 1: Login (Acquiring the Access Token)
1. Open the Swagger UI page in your browser.
2. Find the authentication endpoint (a `POST` request like `/login`).
3. Click **Try it out** and provide your credentials in the request body:
```json
{
  "email": "user@example.com",
  "password": "your_password"
}
```
4. Execute the request. The server will return a response containing an `accessToken` (a string encoded in **Base64** format). Copy this token string.

### Step 2: Authorizing in Swagger
1. Scroll to the very top of the Swagger UI page and click the lock icon button labeled **Authorize**.
2. In the input field, type `Bearer`, then **add a single space**, and paste your copied Base64 token.
   * *Example:* `Bearer CfDJ8J...`
3. Click **Authorize** and close the modal window.

All secure endpoints will now automatically include this token in their headers, allowing you to test the API seamlessly.


## Users credentials
- Admin:
  - Username: admin@gmail.com
  - Password: Admin111#
- Regular user
  - Username: member@gmail.com
  - Password: Member111#


## Contact
* **Author:** Hanna Zaitsava
* **GitHub:** https://github.com/HannaZaitsava/
* **email:** hanna.zaitsava.work@gmail.com