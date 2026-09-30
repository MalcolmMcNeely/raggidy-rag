# Postgres with pgvector, and Voyage AI

The vector store is Postgres with the pgvector extension, and the embedding model is Voyage AI.
Postgres won because it can be read with plain SQL, it already holds keyword search for a later
hybrid search, and most teams already run it. Voyage AI won because it pairs with Claude, its free
allowance covers the corpus many times over, and no official .NET client exists, so the client is
the team's own code.

## Considered options

- Qdrant: a store built only for vectors, with a page that shows each Chunk. One more system, and
  hybrid search later needs its own set-up.
- Ollama with nomic-embed-text: free and local, but the machine has no GPU, it adds a container and
  a model download, and the client comes from a library.
