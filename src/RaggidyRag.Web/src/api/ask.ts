import type { AskResult } from './AskResult'
import { errorText } from './errorText'

export async function ask(question: string): Promise<AskResult> {
  const response = await fetch('/ask', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text: question }),
  })
  if (!response.ok) {
    throw new Error(await errorText(response))
  }
  return (await response.json()) as AskResult
}
