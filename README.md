# CsvToSql
CSV-to-SQL Real-Time ETL Microservices Pipeline (Kafka + .NET 8 + MSSQL)

This project implements a real-time, fault-tolerant ETL pipeline using Apache Kafka, .NET 8 microservices, and SQL Server (MSSQL).
It ingests large CSV datasets, streams records into Kafka, performs validation + transformation, and loads clean data into the database — all with DLQ, retries, idempotency, and high throughput.

🌟 Key Features
✅ Microservices architecture
Service	Responsibility
Ingest.Api	Upload CSV, stream each row to Kafka
Transformer.Worker	Validate + transform CSV rows → typed events
Loader.Worker	Persist clean rows into MSSQL using EF Core
Common	Shared contracts, Kafka wrapper, validation
✅ Event-driven messaging

Kafka topics:

csv.raw.rows → raw CSV events

csv.rows.transformed → validated + enriched events

csv.rows.dlq → invalid rows / processing failures

Idempotent producers

Manual offset commits → prevents data loss

Backpressure support

Dead Letter Queue (DLQ) on schema/parse/validation failures

✅ Enterprise-grade error handling

FluentValidation for schema validation

Try-catch + DLQ fallback

Structured logging (Serilog)

Graceful shutdown & cancellation tokens

Retries with Kafka producer config

✅ Database Layer

SQL Server

Clean normalized schema

EF Core with DbContext & migrations

Idempotent inserts (no duplication)

🚀 Architecture Diagram
[ CSV Upload API ]
       ↓
[ Ingest.Api ] → parses CSV → pushes to Kafka (csv.raw.rows)
       ↓
[ Transformer.Worker ] → validate + map → (csv.rows.transformed)
       ↓               ↳ invalid → (csv.rows.dlq)
[ Loader.Worker ] → writes to MSSQL
       ↓
[ SQL Server ]

🛠️ Tech Stack
Category	Technology
Language	C#, .NET 8
Messaging	Apache Kafka
Database	SQL Server (SSMS / Docker)
ORM	EF Core
Validation	FluentValidation
Logging	Serilog
Docs	Swagger/OpenAPI
Architecture	Microservices + Event-Driven
📁 Folder Structure
CsvToSql.sln
src/
  Common/
    Common.csproj
    Contracts/Contracts.cs
    Messaging/KafkaHelpers.cs
    Observability/Logging.cs
  Ingest.Api/
    Ingest.Api.csproj
    Program.cs
    appsettings.json
    Controllers/CsvController.cs
    Services/IngestService.cs
  Transformer.Worker/
    Transformer.Worker.csproj
    Program.cs
    appsettings.json
    Services/TransformerService.cs
    Validation/RawRowValidator.cs
  Loader.Sql/
    Loader.Sql.csproj
    SalesDbContext.cs
    Entities.cs
    Migrations/20240930_Initial.sql  (optional raw SQL bootstrap)
  Loader.Worker/
    Loader.Worker.csproj
    Program.cs
    appsettings.json
    Services/LoaderService.cs
README.md

Kafka topics used:

csv.raw.rows

csv.rows.transformed

csv.rows.dlq

2️⃣ Start Kafka (local install)
3️⃣ Create Kafka topics
kafka-topics --bootstrap-server localhost:9092 --create --topic csv.raw.rows
kafka-topics --bootstrap-server localhost:9092 --create --topic csv.rows.transformed
kafka-topics --bootstrap-server localhost:9092 --create --topic csv.rows.dlq

4️⃣ Run services
dotnet run --project src/Ingest.Api
dotnet run --project src/Transformer.Worker
dotnet run --project src/Loader.Worker

5️⃣ Upload CSV
curl -F "file=@sales.csv" http://localhost:5117/swagger/index.html => http://localhost:5117/api/ingest/upload

✅ Use Cases

High-volume ETL pipelines

CSV → Kafka → Database ingestion

Real-time data engineering

Distributed microservices architecture

Fault-tolerant data processing with DLQ
