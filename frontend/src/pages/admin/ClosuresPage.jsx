import { useEffect, useState } from 'react'
import { api } from '../../api/client.js'
import { useAuth } from '../../context/AuthContext.jsx'

const emptyForm = { courtId: '', date: '', fullDay: true, startTime: '', endTime: '', reason: '' }

export default function ClosuresPage() {
  const { session } = useAuth()
  const [closures, setClosures] = useState([])
  const [courts, setCourts] = useState([])
  const [loading, setLoading] = useState(true)
  const [form, setForm] = useState(emptyForm)
  const [error, setError] = useState('')

  function load() {
    setLoading(true)
    Promise.all([
      api.get('/admin/closures', { token: session.token }),
      api.get('/admin/courts', { token: session.token })
    ]).then(([c, courtList]) => { setClosures(c); setCourts(courtList) }).finally(() => setLoading(false))
  }
  useEffect(load, []) // eslint-disable-line react-hooks/exhaustive-deps

  function normalizeTime(t) { return t && t.length === 5 ? `${t}:00` : t || null }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    try {
      await api.post('/admin/closures', {
        courtId: form.courtId ? Number(form.courtId) : null,
        date: form.date,
        startTime: form.fullDay ? null : normalizeTime(form.startTime),
        endTime: form.fullDay ? null : normalizeTime(form.endTime),
        reason: form.reason || null
      }, { token: session.token })
      setForm(emptyForm)
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function handleDelete(id) {
    if (!confirm('Remove this closure?')) return
    try {
      await api.del(`/admin/closures/${id}`, { token: session.token })
      load()
    } catch (err) {
      alert(err.message)
    }
  }

  return (
    <div>
      <div style={{ marginBottom: 20 }}>
        <span className="section-eyebrow">Admin</span>
        <h1>Closures</h1>
        <p className="muted">Block off a date (or a time range) for maintenance, holidays, or private events.</p>
      </div>

      <form className="card" onSubmit={handleSubmit}>
        <h3 style={{ marginBottom: 16 }}>Add a closure</h3>
        <div className="field-row">
          <div className="field">
            <label>Date</label>
            <input required type="date" value={form.date} onChange={e => setForm({ ...form, date: e.target.value })} />
          </div>
          <div className="field">
            <label>Court</label>
            <select value={form.courtId} onChange={e => setForm({ ...form, courtId: e.target.value })}>
              <option value="">All courts</option>
              {courts.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
        </div>

        <div className="field">
          <label className="row" style={{ fontWeight: 400 }}>
            <input type="checkbox" checked={form.fullDay} onChange={e => setForm({ ...form, fullDay: e.target.checked })} />
            Full day
          </label>
        </div>

        {!form.fullDay && (
          <div className="field-row">
            <div className="field">
              <label>From</label>
              <input type="time" required={!form.fullDay} value={form.startTime} onChange={e => setForm({ ...form, startTime: e.target.value })} />
            </div>
            <div className="field">
              <label>To</label>
              <input type="time" required={!form.fullDay} value={form.endTime} onChange={e => setForm({ ...form, endTime: e.target.value })} />
            </div>
          </div>
        )}

        <div className="field">
          <label>Reason (optional)</label>
          <input value={form.reason} onChange={e => setForm({ ...form, reason: e.target.value })} />
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        <button className="btn btn-primary" type="submit">Add closure</button>
      </form>

      <div className="card">
        {loading ? <p className="muted">Loading…</p> : (
          <div className="table-wrap">
            <table>
              <thead><tr><th>Date</th><th>Court</th><th>Time</th><th>Reason</th><th></th></tr></thead>
              <tbody>
                {closures.map(c => (
                  <tr key={c.id}>
                    <td>{c.date}</td>
                    <td>{c.courtName || 'All courts'}</td>
                    <td>{c.startTime ? `${c.startTime} – ${c.endTime}` : 'Full day'}</td>
                    <td className="muted">{c.reason || '—'}</td>
                    <td><button className="btn btn-danger btn-sm" onClick={() => handleDelete(c.id)}>Remove</button></td>
                  </tr>
                ))}
                {closures.length === 0 && <tr><td colSpan={5} className="muted text-center" style={{ padding: 24 }}>No closures scheduled.</td></tr>}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
