# RAG Demo

Simple Retrieval-Augmented Generation demo built with ASP.NET Core, Ollama and Qdrant.
The project allows documents to be uploaded, split into smaller chunks, converted into embeddings and stored in a vector database.
User questions are converted into embeddings and matched against stored document chunks. The most relevant fragments are then passed to a local LLM as context.

## Technologies

- ASP.NET Core
- Ollama
- Qdrant
- Docker
- Docker Compose

## Models

Embedding model:

```text
embeddinggemma
```

Chat model:

```text
qwen3:4b
```

## Architecture

```text
Document
   |
Text extraction
   |
Chunking
   |
Ollama Embeddings
   |
Qdrant
```

Question flow:

```text
Question
   |
Embedding
   |
Qdrant search
   |
Relevant document chunks
   |
Ollama LLM
   |
Answer
```

## Features

- text document upload
- document chunking
- local embedding generation
- vector storage in Qdrant
- semantic search
- score threshold filtering
- local LLM responses
- document sources returned with answers

## Running Qdrant

Start Qdrant using Docker Compose:

```bash
docker compose up -d
```

Qdrant dashboard:

```text
http://localhost:6333/dashboard
```

## Running Ollama

Required models:

```bash
ollama pull embeddinggemma
ollama pull qwen3:4b
```

Check installed models:

```bash
ollama list
```

Ollama API runs by default on:

```text
http://localhost:11434
```

## Running the API

```bash
dotnet run --project src/RagApi
```

## Uploading a document

Endpoint:

```http
POST /api/documents/upload
```

Request type:

```text
multipart/form-data
```

Example:

```text
file = document.txt
```

The document is split into chunks and each chunk is stored in Qdrant together with its embedding and metadata.

## Asking questions

Endpoint:

```http
POST /api/rag/ask
```

Example request:

```json
{
  "question": "What is Kubernetes used for?"
}
```

Example response:

```json
{
  "answer": "Kubernetes is used to orchestrate containerized applications.",
  "sources": [
    {
      "fileName": "kubernetes_operations_guide.txt",
      "chunkIndex": 2,
      "text": "A Deployment manages ReplicaSets and Pods...",
      "score": 0.81
    }
  ]
}
```

## Configuration

Main configuration is stored in:

```text
appsettings.json
```

Example:

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "EmbeddingModel": "embeddinggemma",
    "ChatModel": "qwen3:4b"
  },
  "Qdrant": {
    "Host": "localhost",
    "Port": 6334,
    "CollectionName": "documents",
    "VectorSize": 768
  },
  "Rag": {
    "SearchLimit": 3,
    "ScoreThreshold": 0.5,
    "ChunkSize": 1200,
    "ChunkOverlap": 200
  }
}
```
