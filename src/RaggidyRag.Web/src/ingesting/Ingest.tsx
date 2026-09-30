import { useState } from 'react'
import { ingest } from '../api/ingest'
import type { IngestResult } from '../api/IngestResult'

type Ingesting =
  | { state: 'idle' }
  | { state: 'running' }
  | { state: 'done'; counts: IngestResult }
  | { state: 'failed'; error: string }

export function Ingest() {
  const [ingesting, setIngesting] = useState<Ingesting>({ state: 'idle' })

  async function press() {
    setIngesting({ state: 'running' })
    try {
      setIngesting({ state: 'done', counts: await ingest() })
    } catch (error) {
      setIngesting({ state: 'failed', error: error instanceof Error ? error.message : String(error) })
    }
  }

  return (
    <section>
      <button type="button" onClick={press} disabled={ingesting.state === 'running'}>
        Ingest
      </button>
      {ingesting.state === 'running' && <p role="status">Ingesting…</p>}
      {ingesting.state === 'done' && (
        <p>Documents read: {ingesting.counts.documentsRead}. Chunks stored: {ingesting.counts.chunksStored}.</p>
      )}
      {ingesting.state === 'failed' && <p role="alert">{ingesting.error}</p>}
    </section>
  )
}
