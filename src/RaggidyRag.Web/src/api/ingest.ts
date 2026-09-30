import type { IngestResult } from './IngestResult'
import { errorText } from './errorText'

export async function ingest(): Promise<IngestResult> {
  const response = await fetch('/ingest', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: '{}',
  })
  if (!response.ok) {
    throw new Error(await errorText(response))
  }
  return (await response.json()) as IngestResult
}
