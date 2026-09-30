import type { RetrievedChunk } from '../api/RetrievedChunk'

export function RetrievedChunks({ chunks }: { chunks: RetrievedChunk[] }) {
  return (
    <ol aria-label="Retrieved Chunks">
      {chunks.map((chunk) => (
        <li key={chunk.number}>
          <p>
            [{chunk.number}] {chunk.documentPath} — {chunk.headingTrail} · score {chunk.score.toFixed(3)}
          </p>
          <p>{chunk.text}</p>
        </li>
      ))}
    </ol>
  )
}
