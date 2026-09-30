import { cleanup, render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { App } from './App'

const nothingIngested = {
  answer: 'Nothing has been ingested yet. Press Ingest first.',
  citations: [],
  retrievedChunks: [],
}

const answered = {
  answer: 'An outbox holds each message [1]. A saga keeps its state [2].',
  citations: [
    { number: 1, documentPath: 'outbox.md', headingTrail: 'Outbox' },
    { number: 2, documentPath: 'adr/0001-use-sagas.md', headingTrail: 'Use sagas > Why' },
  ],
  retrievedChunks: [
    { number: 1, documentPath: 'outbox.md', headingTrail: 'Outbox', score: 0.1, text: 'An outbox holds each message.' },
    { number: 2, documentPath: 'adr/0001-use-sagas.md', headingTrail: 'Use sagas > Why', score: 0.2, text: 'A saga keeps its state.' },
  ],
}

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

const counts = { documentsRead: 117, chunksStored: 340 }

async function pressAsk(question: string) {
  const person = userEvent.setup()
  render(<App />)
  await person.type(screen.getByRole('textbox', { name: 'Question' }), question)
  await person.click(screen.getByRole('button', { name: 'Ask' }))
}

async function pressIngest() {
  const person = userEvent.setup()
  render(<App />)
  await person.click(screen.getByRole('button', { name: 'Ingest' }))
}

describe('the page', () => {
  it('shows the counts after a person presses Ingest', async () => {
    // Arrange
    vi.stubGlobal('fetch', () => Promise.resolve(Response.json(counts)))

    // Act
    await pressIngest()

    // Assert
    expect((await screen.findByText('Documents read: 117. Chunks stored: 340.')).textContent).toBe('Documents read: 117. Chunks stored: 340.')
  })

  it('says that Ingest is running until the API answers', async () => {
    // Arrange
    let letGo!: (answered: Response) => void
    vi.stubGlobal('fetch', () => new Promise<Response>((resolve) => { letGo = resolve }))

    // Act
    await pressIngest()

    // Assert
    expect((await screen.findByRole('status')).textContent).toBe('Ingesting…')
    letGo(Response.json(counts))
    await screen.findByText('Documents read: 117. Chunks stored: 340.')
  })

  it('names the service that made Ingest fail', async () => {
    // Arrange
    const problem = { title: 'Postgres failed', detail: 'Postgres refused the connection.', status: 502 }
    vi.stubGlobal('fetch', () => Promise.resolve(Response.json(problem, {
      status: 502,
      headers: { 'Content-Type': 'application/problem+json' },
    })))

    // Act
    await pressIngest()

    // Assert
    expect((await screen.findByRole('alert')).textContent).toBe('Postgres failed: Postgres refused the connection.')
  })

  it('shows the Answer after a person asks a Question', async () => {
    // Arrange
    vi.stubGlobal('fetch', () => Promise.resolve(Response.json(nothingIngested)))

    // Act
    await pressAsk('What is a saga?')

    // Assert
    expect((await screen.findByText(nothingIngested.answer)).textContent).toBe(nothingIngested.answer)
  })

  it('shows the Answer with its Citation marks', async () => {
    // Arrange
    vi.stubGlobal('fetch', () => Promise.resolve(Response.json(answered)))

    // Act
    await pressAsk('What does an outbox hold?')

    // Assert
    expect((await screen.findByText(answered.answer)).textContent).toBe(answered.answer)
  })

  it('lists each Citation with its Document path and Heading trail under the Answer', async () => {
    // Arrange
    vi.stubGlobal('fetch', () => Promise.resolve(Response.json(answered)))

    // Act
    await pressAsk('What does an outbox hold?')

    // Assert
    const list = await screen.findByRole('list', { name: 'Citations' })
    expect(within(list).getAllByRole('listitem').map((citation) => citation.textContent)).toEqual([
      '[1] outbox.md — Outbox',
      '[2] adr/0001-use-sagas.md — Use sagas > Why',
    ])
  })

  it('lists each retrieved Chunk with its score under the Answer', async () => {
    // Arrange
    const retrieved = {
      answer: '',
      citations: [],
      retrievedChunks: [
        { number: 1, documentPath: 'outbox.md', headingTrail: 'Outbox', score: 0.1234, text: 'An outbox holds each message.' },
        { number: 2, documentPath: 'adr/0001-use-sagas.md', headingTrail: 'Use sagas > Why', score: 0.5, text: 'A saga keeps its state.' },
      ],
    }
    vi.stubGlobal('fetch', () => Promise.resolve(Response.json(retrieved)))

    // Act
    await pressAsk('What does an outbox hold?')

    // Assert
    const list = await screen.findByRole('list', { name: 'Retrieved Chunks' })
    expect(within(list).getAllByRole('listitem').map((chunk) => chunk.textContent)).toEqual([
      '[1] outbox.md — Outbox · score 0.123An outbox holds each message.',
      '[2] adr/0001-use-sagas.md — Use sagas > Why · score 0.500A saga keeps its state.',
    ])
  })

  it('says that Ask is running until the API answers', async () => {
    // Arrange
    let letGo!: (answered: Response) => void
    vi.stubGlobal('fetch', () => new Promise<Response>((resolve) => { letGo = resolve }))

    // Act
    await pressAsk('What is a saga?')

    // Assert
    expect((await screen.findByRole('status')).textContent).toBe('Asking…')
    letGo(Response.json(nothingIngested))
    await screen.findByText(nothingIngested.answer)
  })

  it('names the service that made Ask fail', async () => {
    // Arrange
    const problem = { title: 'Claude failed', detail: 'The key was refused.', status: 502 }
    vi.stubGlobal('fetch', () => Promise.resolve(Response.json(problem, {
      status: 502,
      headers: { 'Content-Type': 'application/problem+json' },
    })))

    // Act
    await pressAsk('What is a saga?')

    // Assert
    expect((await screen.findByRole('alert')).textContent).toBe('Claude failed: The key was refused.')
  })

  it('shows the status when the API fails without problem details', async () => {
    // Arrange
    vi.stubGlobal('fetch', () => Promise.resolve(new Response('Bad Gateway', { status: 502, statusText: 'Bad Gateway' })))

    // Act
    await pressAsk('What is a saga?')

    // Assert
    expect((await screen.findByRole('alert')).textContent).toBe('502 Bad Gateway')
  })

  it('shows what the browser reports when the call never reaches the API', async () => {
    // Arrange
    vi.stubGlobal('fetch', () => Promise.reject(new TypeError('Failed to fetch')))

    // Act
    await pressAsk('What is a saga?')

    // Assert
    expect((await screen.findByRole('alert')).textContent).toBe('Failed to fetch')
  })
})
