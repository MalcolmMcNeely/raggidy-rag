import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { App } from './App'

const nothingIngested = {
  answer: 'Nothing has been ingested yet. Press Ingest first.',
  citations: [],
  retrievedChunks: [],
}

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

async function pressAsk(question: string) {
  const person = userEvent.setup()
  render(<App />)
  await person.type(screen.getByRole('textbox', { name: 'Question' }), question)
  await person.click(screen.getByRole('button', { name: 'Ask' }))
}

describe('the page', () => {
  it('shows the Answer after a person asks a Question', async () => {
    // Arrange
    vi.stubGlobal('fetch', () => Promise.resolve(Response.json(nothingIngested)))

    // Act
    await pressAsk('What is a saga?')

    // Assert
    expect((await screen.findByText(nothingIngested.answer)).textContent).toBe(nothingIngested.answer)
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

  it('shows the problem details title and detail when the API fails', async () => {
    // Arrange
    const problem = { title: 'Voyage AI failed', detail: 'The embedding call was refused.', status: 502 }
    vi.stubGlobal('fetch', () => Promise.resolve(Response.json(problem, {
      status: 502,
      headers: { 'Content-Type': 'application/problem+json' },
    })))

    // Act
    await pressAsk('What is a saga?')

    // Assert
    expect((await screen.findByRole('alert')).textContent).toBe('Voyage AI failed: The embedding call was refused.')
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
