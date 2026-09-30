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

**Question**:
The text a person types to ask something of the Documents.
_Avoid_: Query, request, input

**Retrieve**:
The step that finds the Chunks that fit a Question best.
_Avoid_: Lookup, fetch, find

**Prompt**:
The text the code sends to the model: the Question, the retrieved Chunks, each with a number, and
the instructions for answering from them.
_Avoid_: Message, context, template

**Answer**:
The text the model writes from the retrieved Chunks, with a Citation after each fact.
_Avoid_: Response, reply, completion, output

**Citation**:
A numbered mark in an Answer, such as `[2]`, that points at the Chunk a fact came from. It shows as
the Document's path and the Chunk's Heading trail.
_Avoid_: Reference, source, footnote, link
