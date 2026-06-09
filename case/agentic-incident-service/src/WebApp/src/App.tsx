import React, { useState, useEffect } from 'react'
import IncidentList from './components/IncidentList'
import IncidentDetails from './components/IncidentDetails'

export default function App() {
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [dark, setDark] = useState(() => document.documentElement.getAttribute('data-theme') === 'dark')

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light')
  }, [dark])

  return (
    <div style={{ fontFamily: 'system-ui, sans-serif', padding: 16, maxWidth: 1100, margin: '0 auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ margin: 0 }}>Agentic Incident Service</h1>
          <p style={{ marginTop: 4, color: 'var(--text-muted)' }}>
            GitHub Copilot Hackathon.
          </p>
        </div>
        <button
          onClick={() => setDark(d => !d)}
          style={{
            padding: '6px 14px',
            borderRadius: 8,
            border: '1px solid var(--border)',
            background: 'var(--bg-card)',
            color: 'var(--text)',
            cursor: 'pointer',
            fontSize: 14,
          }}
        >
          {dark ? '☀️ Light' : '🌙 Dark'}
        </button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
        <IncidentList selectedId={selectedId} onSelect={setSelectedId} />
        <IncidentDetails id={selectedId} />
      </div>
    </div>
  )
}
