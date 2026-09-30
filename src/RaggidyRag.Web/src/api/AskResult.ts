import type { RetrievedChunk } from './RetrievedChunk'

export type AskResult = {
  answer: string
  retrievedChunks: RetrievedChunk[]
}
