import { useState, type FormEvent } from 'react'
import { ask } from '../api/ask'
import type { AskResult } from '../api/AskResult'
import { RetrievedChunks } from './RetrievedChunks'

type Asking =
  | { state: 'idle' }
  | { state: 'running' }
  | { state: 'answered'; result: AskResult }
  | { state: 'failed'; error: string }

export function Ask() {
  const [question, setQuestion] = useState('')
  const [asking, setAsking] = useState<Asking>({ state: 'idle' })

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setAsking({ state: 'running' })
    try {
      setAsking({ state: 'answered', result: await ask(question) })
    } catch (error) {
      setAsking({ state: 'failed', error: error instanceof Error ? error.message : String(error) })
    }
  }

  return (
    <form onSubmit={submit}>
      <label>
        Question
        <textarea value={question} onChange={(event) => setQuestion(event.target.value)} />
      </label>
      <button type="submit" disabled={asking.state === 'running'}>
        Ask
      </button>
      {asking.state === 'running' && <p role="status">Asking…</p>}
      {asking.state === 'answered' && (
        <>
          <p>{asking.result.answer}</p>
          {asking.result.retrievedChunks.length > 0 && <RetrievedChunks chunks={asking.result.retrievedChunks} />}
        </>
      )}
      {asking.state === 'failed' && <p role="alert">{asking.error}</p>}
    </form>
  )
}
