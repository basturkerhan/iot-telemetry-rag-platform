# Real-Time IoT Telemetry Stream & Generative AI (RAG) Platform

![Image 1](images/image-01.png)
![Image 2](images/image-02.png)

An enterprise-grade, end-to-end telemetry monitoring and intelligent anomaly detection demo platform. The system collects high-frequency virtual sensor data from virtual industrial IoT edge devices , processes it through asynchronous messaging pipelines, stores vector embeddings in **PostgreSQL (pgvector)**, and performs **Generative AI / RAG (Retrieval-Augmented Generation)** analysis for explainable real-time insights.

---

## 🏛️ Architectural Style & Design Principles

* **Event-Driven Architecture (EDA):** Ingestion, processing, and storage pipelines are completely decoupled using an asynchronous event-based backbone.
* **Vertical Slice Architecture:** Rather than traditional layered/monolithic architectures, the solution is structured around feature slices (`Bridge`, `Ingestion`, `Analytics`), ensuring high cohesion and low coupling.
* **CQRS Pattern:** Commands and Queries are cleanly separated via MassTransit Mediator.
* **Extensible & Pluggable AI and Embedding Services (SOLID):** 
  * The solution abstracts all artificial intelligence and vector generation operations behind dedicated interfaces (`IRagService` and `IEmbeddingService`).
  * Any alternative AI provider (e.g., OpenAI, Anthropic Claude, local Ollama/LlamaSharp) or embedding engine can be plugged in by implementing the respective interface without modifying existing business logic or API endpoints.
* **Hybrid Local/Cloud RAG Pipeline:** Embedding calculations are performed locally on the CPU (zero API cost, zero network latency), while LLM reasoning is handled dynamically with compact context formatting.

---

## 🛠️ Tech Stack & Key Components

### Backend & Infrastructure
* **.NET 10 (C#):** Core backend engine built with ASP.NET Core Minimal APIs for minimal overhead.
* **MQTT & MQTTnet:** Simulates field hardware and publishes telemetry payloads over lightweight protocols.
* **Eclipse Mosquitto:** Edge-level lightweight MQTT message broker.
* **RabbitMQ & MassTransit:** Enterprise message routing, prefetch control, Consumer batching, and Mediator dispatching.
* **PostgreSQL 16 & pgvector:** Unified relational and high-dimensional vector database supporting vector operations directly in SQL.
* **Entity Framework Core:** High-throughput data access and dimensioned vector mapping.
* **SignalR:** Server-to-Client streaming pushing live telemetry readings every 3 seconds to connected dashboards via WebSockets.
* **Local Embeddings (ONNX):** In-process 384-dimensional vector embedding engine running without external network dependencies.
* **Google Gemini 3.6 Flash:** Large Language Model utilized for semantic reasoning and context-grounded anomaly analysis.
* **Docker & Docker Compose:** Containerized environment orchestrating Mosquitto, RabbitMQ, and PostgreSQL instances.

### Frontend
* **Next.js (React) & TypeScript:** Modern, type-safe reactive dashboard.
* **Tailwind CSS:** Industrial dark-mode UI with low latency rendering.
* **@microsoft/signalr:** Handles bidirectional WebSocket connections, real-time telemetry stream subscriptions, and conversational AI interactions.

---

## 🔄 End-to-End Data Pipeline

```text
[IoT Device Simulators]
         │ (MQTT Protocol)
         ▼
  [Mosquitto Broker]
         │ (Background MQTT Listener)
         ▼
[IoT.Platform: Bridge Service]
         │ (Publish: TelemetryReceivedEvent)
         ▼
   [RabbitMQ Exchange]
         │ (Prefetch: 200, Batch Limit: 100 or 3s)
         ▼
[MassTransit Batch Consumer]
   ├── 1. Local Vectorization (IEmbeddingService - 384-dim)
   └── 2. Bulk Insert (EF Core AddRangeAsync)
         │
         ▼
[PostgreSQL + pgvector] ◄─── (HNSW Index / Cosine Distance)
         │
 ┌───────┴──────────────────────────────────────────┐
 │                                                  │
 │ (3s Streaming via SignalR)                       │ (Natural Language RAG Query)
 ▼                                                  ▼
[Device Telemetry Panel]                     [Ask AI Assistant Panel]
(Live Temp, Hum, Vib cards)                  ├── 1. Query Vectorization (IEmbeddingService)
                                             ├── 2. Top-10 Vector Similarity Retrieval
                                             ├── 3. Statistical Baseline Aggregation
                                             └── 4. LLM Reasoning (IRagService)
```

* **Edge Simulation:** Independent device workers (DEV-101, DEV-102, DEV-103) generate synthetic temperature, humidity, and vibration readings to MQTT topics.

* **Protocol Bridging:** A hosted background worker consumes MQTT packets, normalizes payloads, and publishes them as enterprise events (TelemetryReceivedEvent) to RabbitMQ.

* **High-Throughput Batch Ingestion:** MassTransit buffers messages in-memory (up to 100 items or 3-second windows). Vectors are generated locally via IEmbeddingService and written to PostgreSQL in single multi-row SQL insert operations.

* **Live Stream Subscription:** The SignalR Hub streams latest state snapshots per device to active clients every 3 seconds using composite indexes (DeviceId, Timestamp DESC).

* **RAG-Powered Anomaly Querying:** Natural language questions are converted into search vectors. Relevant contextual logs retrieved via HNSW Cosine Distance search are combined with statistical 1-hour baselines and passed to IRagService to generate grounded, explainable answers.

---

## ⚡ Performance & Database Optimizations

* **HNSW Vector Indexing:** Employs Hierarchical Navigable Small World graphs (`vector_cosine_ops`) over 384-dimensional vectors to achieve $O(\log N)$ query speed, preventing full-table scans.
* **Composite B-Tree Indexes:** Defined on `(DeviceId, Timestamp DESC)` for multi-tenant and time-series lookups.
* **Memory & I/O Batching:** Replaces discrete single-row insertions with chunked batch writes, cutting database connection overhead by over 80%.
* **Token-Optimized Context Formatting:** Telemetry contexts are arranged in short lines separated by delimiters.

---

## 🚀 Getting Started

### Prerequisites
* Docker & Docker Compose


### Set Environment Variables
Create a .env file using the .env.example file and prepare its contents.

### Start Containers

```bash
docker compose up -d
```

Navigate to `http://localhost:3000` to interact with the real-time telemetry feed and the AI Assistant.