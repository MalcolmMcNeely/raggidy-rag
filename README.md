# Raggidy RAG

Raggidy RAG answers questions about a folder of documents, and names the file each answer came from.

It is a project to learn RAG: retrieval-augmented generation. The app finds the parts of the
documents that fit a question, and gives them to a model to answer from.

## Status

The first slice runs: Ingest the Edict docs, then ask a Question and get an Answer with Citations.

## Layout

- `src/RaggidyRag.AppHost`: the Aspire app host. It starts every service and container.
- `src/RaggidyRag.ServiceDefaults`: the logging, health checks and telemetry each service shares.
- `docs/agents/`: the rules and checks the Skillworks loop reads.

## The plan

Each step is one thing to learn:

1. Chunking: cut each document into small parts.
2. Embed and store: turn each part into numbers that hold its meaning, and save them.
3. Retrieve: find the parts that fit a question.
4. Answer with citations: the model answers from those parts and names its sources.
5. Hybrid search: search by keyword and by meaning together.
6. Reranking: sort the results again so the best come first.
7. Evaluation: score the answers against a fixed set of questions.
