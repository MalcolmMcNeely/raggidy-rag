import { Ask } from './asking/Ask'
import { Ingest } from './ingesting/Ingest'

export function App() {
  return (
    <main>
      <h1>Raggidy RAG</h1>
      <Ingest />
      <Ask />
    </main>
  )
}
