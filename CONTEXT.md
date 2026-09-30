# Raggidy RAG

Raggidy RAG answers a question from a folder of documents, and names the document each fact came
from. It exists to show each step of retrieval-augmented generation in plain code.

## Language

**Document**:
One Markdown file in the folder the app reads. The first folder is the `docs` folder of the Edict
repo, ADRs included.
_Avoid_: File, page, article, source

**Chunk**:
One piece of a Document, cut at a heading and then by size. It is the unit that is embedded, stored
and retrieved, and it keeps the Document it came from and its Heading trail.
_Avoid_: Segment, passage, fragment, split, piece

**Heading trail**:
The headings above a Chunk, from the Document's title down, such as "Saga model > Lifetime".
_Avoid_: Breadcrumb, path, outline

**Ingest**:
The step that reads every Document in the folder, cuts it into Chunks, embeds each Chunk and stores
it.
_Avoid_: Load, index, import, sync
