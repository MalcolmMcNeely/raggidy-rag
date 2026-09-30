# Own code for each pipeline step

Raggidy RAG exists to show each step of retrieval-augmented generation in code the team wrote, so
that a reader can explain chunking, embedding, retrieval and the prompt in an interview. A RAG
library such as Kernel Memory would do all four in a few lines and hide them. So the pipeline steps
are hand-written, and a library is used only at a boundary: Microsoft.Extensions.AI for the model
calls and Microsoft.Extensions.VectorData for the store.

## Consequences

An agent that reaches for a library to chunk, embed, retrieve or build the Prompt is undoing the
reason the repo exists.
