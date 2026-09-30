type ProblemDetails = {
  title?: string
  detail?: string
}

export async function errorText(response: Response): Promise<string> {
  const problem = await problemDetailsOf(response)
  if (problem?.title) {
    return problem.detail ? `${problem.title}: ${problem.detail}` : problem.title
  }
  return `${response.status} ${response.statusText}`.trim()
}

async function problemDetailsOf(response: Response): Promise<ProblemDetails | undefined> {
  try {
    const body: unknown = await response.json()
    if (typeof body !== 'object' || body === null) {
      return undefined
    }
    const { title, detail } = body as Record<string, unknown>
    return {
      title: typeof title === 'string' ? title : undefined,
      detail: typeof detail === 'string' ? detail : undefined,
    }
  } catch {
    return undefined
  }
}
