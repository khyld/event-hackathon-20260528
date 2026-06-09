import React, { useEffect, useState } from 'react'
import { HttpError, api, type IncidentDetails as Details, type Recommendation } from '../api'

export default function IncidentDetails({ id }: { id: string | null }) {
  const [details, setDetails] = useState<Details | null>(null)
  const [rec, setRec] = useState<Recommendation | null>(null)
  const [recError, setRecError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [notFound, setNotFound] = useState(false)

  useEffect(() => {
    if (!id) {
      setDetails(null)
      setRec(null)
      setRecError(null)
      setError(null)
      setNotFound(false)
      return
    }

    const run = async () => {
      try {
        setLoading(true)
        setError(null)
        setRecError(null)
        setNotFound(false)
        setDetails(null)
        setRec(null)

        const d = await api.getIncident(id)
        setDetails(d)

        try {
          const r = await api.getRecommendation(id)
          setRec(r)
        } catch (recErr: any) {
          setRecError(recErr?.message ?? 'Failed to load recommendation')
        }
      } catch (e: any) {
        if (e instanceof HttpError && e.status === 404) {
          setNotFound(true)
          return
        }
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
  if (notFound) return <div style={{ color: '#888' }}>Incident not found.</div>
  if (!details) return <div>No details.</div>

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
        <div style={{ color: '#555', fontSize: 12, marginTop: 4 }}>
          Observed: {new Date(details.observedAt).toLocaleString()}
        </div>
        {details.tags.length > 0 && (
          <div style={{ marginTop: 6, display: 'flex', gap: 4, flexWrap: 'wrap' }}>
            {details.tags.map(t => (
              <span key={t} style={{ background: '#eee', borderRadius: 4, padding: '2px 6px', fontSize: 12 }}>{t}</span>
            ))}
          </div>
        )}
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

      {recError && (
        <div style={{ marginTop: 12, padding: 12, border: '1px solid #ddd', borderRadius: 8, color: 'crimson' }}>
          <strong>Recommendation</strong>
          <div style={{ marginTop: 6 }}>Failed to load recommendation.</div>
        </div>
      )}
    </div>
  )
}
