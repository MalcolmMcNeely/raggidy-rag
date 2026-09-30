import type { Citation } from '../api/Citation'

export function Citations({ citations }: { citations: Citation[] }) {
  return (
    <ol aria-label="Citations">
      {citations.map((citation) => (
        <li key={citation.number}>
          [{citation.number}] {citation.documentPath} — {citation.headingTrail}
        </li>
      ))}
    </ol>
  )
}
