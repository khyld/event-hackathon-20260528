import React, { useState } from 'react'
import IncidentList from './components/IncidentList'
import IncidentDetails from './components/IncidentDetails'

export default function App() {
  const [selectedId, setSelectedId] = useState<string | null>(null)

  return (
    <div style={{ fontFamily: 'system-ui, sans-serif', padding: 16, maxWidth: 1100, margin: '0 auto' }}>
      <h1>Agentic Incident Service</h1>
      <p style={{ marginTop: 0, color: '#555' }}>
        GitHub Copilot Hackathon.
      </p>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
        <IncidentList selectedId={selectedId} onSelect={setSelectedId} />
        <IncidentDetails id={selectedId} />
      </div>
    </div>
  )
}
