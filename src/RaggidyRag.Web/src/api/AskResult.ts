import type { Citation } from './Citation'
import type { RetrievedChunk } from './RetrievedChunk'

export type AskResult = {
  answer: string
  citations: Citation[]
  retrievedChunks: RetrievedChunk[]
}
