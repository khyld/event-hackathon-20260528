export type Incident = {
  id: string
  title: string
}

export type IncidentDetails = Incident & {
  severity: string
  system: string
  tags: string[]
  observedAt: string
  category: string
  priority: string
  description: string
  reason: string
}

export type Recommendation = {
  incidentId: string
  summary: string
  nextAction: string
  confidence: string
}

export class HttpError extends Error {
  constructor(public readonly status: number, message: string) {
    super(message)
    this.name = 'HttpError'
  }
}

const API_BASE = import.meta.env.VITE_API_BASE || ''

async function http<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...(init?.headers || {}) },
    ...init,
  })
  if (!res.ok) {
    const msg = await res.text()
    throw new HttpError(res.status, `HTTP ${res.status}: ${msg}`)
  }
  return res.json() as Promise<T>
}

export const api = {
  listIncidents: () => http<Incident[]>('/api/incidents'),
  getIncident: (id: string) => http<IncidentDetails>(`/api/incidents/${id}`),
  getRecommendation: (id: string) => http<Recommendation>(`/api/incidents/${id}/recommendation`),
  createIncident: (payload: { title: string; severity: string; system: string; description: string; tags: string[] }) =>
    http(`/api/incidents`, { method: 'POST', body: JSON.stringify(payload) }),
}
