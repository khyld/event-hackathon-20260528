import React, { useEffect, useState } from 'react'
import { api, type IncidentDetails as Details, type Recommendation } from '../api'

export default function IncidentDetails({ id }: { id: string | null }) {
  const [details, setDetails] = useState<Details | null>(null)
  const [rec, setRec] = useState<Recommendation | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!id) return

    const run = async () => {
      try {
        setLoading(true)
        setError(null)
        const d = await api.getIncident(id)
        setDetails(d)
        const r = await api.getRecommendation(id)
        setRec(r)
      } catch (e: any) {
        setError(e?.message ?? 'Unknown error')
      } finally {
        setLoading(false)
      }
    }

    run()
  }, [id])

  if (!id) return <div>Select an incident.</div>
  if (loading) return <div>Loading details…</div>
  if (error) return <div style={{ color: 'crimson' }}>Error: {error}</div>
  if (!details) return <div>No data.</div>

  return (
    <div>
      <h2 style={{ marginTop: 0 }}>Details</h2>
      <div style={{ padding: 12, border: '1px solid #ddd', borderRadius: 8 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between' }}>
          <strong>{details.id}</strong>
          <span>{details.priority}</span>
        </div>
        <div style={{ marginTop: 6 }}>{details.title}</div>
        <div style={{ color: '#555', fontSize: 12, marginTop: 6 }}>
          {details.system} • {details.severity} • {details.category}
        </div>
        <div style={{ marginTop: 10 }}>
          <strong>Description</strong>
          <div style={{ color: '#333' }}>{details.description}</div>
        </div>
        <div style={{ marginTop: 10 }}>
          <strong>Triage reason</strong>
          <div style={{ color: '#333' }}>{details.reason}</div>
        </div>
      </div>

      {rec && (
        <div style={{ marginTop: 12, padding: 12, border: '1px solid #ddd', borderRadius: 8 }}>
          <strong>Recommendation</strong>
          <div style={{ marginTop: 6, color: '#333' }}>{rec.summary}</div>
          <div style={{ marginTop: 6 }}>{rec.nextAction}</div>
          <div style={{ marginTop: 6, color: '#555', fontSize: 12 }}>Confidence: {rec.confidence}</div>
        </div>
      )}
    </div>
  )
}
