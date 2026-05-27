import React, { useEffect, useState } from 'react'
import { api, type Incident } from '../api'

export default function IncidentList({
  selectedId,
  onSelect,
}: {
  selectedId: string | null
  onSelect: (id: string) => void
}) {
  const [items, setItems] = useState<Incident[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const run = async () => {
      try {
        setLoading(true)
        const data = await api.listIncidents()
        setItems(data)
        if (!selectedId && data.length > 0) onSelect(data[0].id)
      } catch (e: any) {
        setError(e?.message ?? 'Unknown error')
      } finally {
        setLoading(false)
      }
    }
    run()
  }, [])

  if (loading) return <div>Loading incidents…</div>
  if (error) return <div style={{ color: 'crimson' }}>Error: {error}</div>

  return (
    <div>
      <h2 style={{ marginTop: 0 }}>Incidents</h2>
      <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
        {items.map(i => {
          const active = i.id === selectedId
          return (
            <li key={i.id}>
              <button
                onClick={() => onSelect(i.id)}
                style={{
                  width: '100%',
                  textAlign: 'left',
                  padding: 10,
                  marginBottom: 8,
                  borderRadius: 8,
                  border: '1px solid #ddd',
                  background: active ? '#f0f6ff' : 'white',
                  cursor: 'pointer',
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <strong>{i.id}</strong>
                  <span>{i.priority}</span>
                </div>
                <div>{i.title}</div>
                <div style={{ color: '#555', fontSize: 12 }}>
                  {i.system} • {i.severity} • {i.category}
                </div>
              </button>
            </li>
          )
        })}
      </ul>
    </div>
  )
}
