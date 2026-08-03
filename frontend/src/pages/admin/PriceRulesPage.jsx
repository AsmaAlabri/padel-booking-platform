import { useEffect, useState } from 'react'
import { api } from '../../api/client.js'
import { useAuth } from '../../context/AuthContext.jsx'

const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
const emptyForm = { name: '', dayOfWeek: '', startTime: '00:00', endTime: '23:59', pricePerHour: '', isDefault: false, isActive: true }

export default function PriceRulesPage() {
  const { session } = useAuth()
  const [rules, setRules] = useState([])
  const [loading, setLoading] = useState(true)
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState(null)
  const [error, setError] = useState('')

  function load() {
    setLoading(true)
    api.get('/admin/price-rules', { token: session.token }).then(setRules).finally(() => setLoading(false))
  }
  useEffect(load, []) // eslint-disable-line react-hooks/exhaustive-deps

  function normalizeTime(t) { return t && t.length === 5 ? `${t}:00` : t }

  function startEdit(r) {
    setEditingId(r.id)
    setForm({
      name: r.name,
      dayOfWeek: r.dayOfWeek ?? '',
      startTime: r.startTime.slice(0, 5),
      endTime: r.endTime.slice(0, 5),
      pricePerHour: r.pricePerHour,
      isDefault: r.isDefault,
      isActive: r.isActive
    })
  }

  function cancelEdit() { setEditingId(null); setForm(emptyForm) }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    const payload = {
      name: form.name,
      dayOfWeek: form.dayOfWeek === '' ? null : form.dayOfWeek,
      startTime: normalizeTime(form.startTime),
      endTime: normalizeTime(form.endTime),
      pricePerHour: Number(form.pricePerHour),
      isDefault: form.isDefault
    }
    try {
      if (editingId) {
        await api.put(`/admin/price-rules/${editingId}`, { ...payload, isActive: form.isActive }, { token: session.token })
      } else {
        await api.post('/admin/price-rules', payload, { token: session.token })
      }
      cancelEdit()
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function handleDelete(id) {
    if (!confirm('Delete this price rule?')) return
    try {
      await api.del(`/admin/price-rules/${id}`, { token: session.token })
      load()
    } catch (err) {
      alert(err.message)
    }
  }

  return (
    <div>
      <div style={{ marginBottom: 20 }}>
        <span className="section-eyebrow">Admin</span>
        <h1>Price rules</h1>
        <p className="muted">The most specific matching rule wins. Keep one rule marked "default" as the fallback price.</p>
      </div>

      <form className="card" onSubmit={handleSubmit}>
        <h3 style={{ marginBottom: 16 }}>{editingId ? 'Edit price rule' : 'Add a price rule'}</h3>
        <div className="field-row">
          <div className="field">
            <label>Name</label>
            <input required value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} />
          </div>
          <div className="field">
            <label>Day of week</label>
            <select value={form.dayOfWeek} onChange={e => setForm({ ...form, dayOfWeek: e.target.value })}>
              <option value="">Every day</option>
              {DAYS.map(d => <option key={d} value={d}>{d}</option>)}
            </select>
          </div>
        </div>
        <div className="field-row">
          <div className="field">
            <label>From</label>
            <input type="time" required value={form.startTime} onChange={e => setForm({ ...form, startTime: e.target.value })} />
          </div>
          <div className="field">
            <label>To</label>
            <input type="time" required value={form.endTime} onChange={e => setForm({ ...form, endTime: e.target.value })} />
          </div>
        </div>
        <div className="field-row">
          <div className="field">
            <label>Price per hour (OMR)</label>
            <input required type="number" step="0.001" min="0" value={form.pricePerHour} onChange={e => setForm({ ...form, pricePerHour: e.target.value })} />
          </div>
          <div className="field">
            <label className="row" style={{ fontWeight: 400, marginTop: 30 }}>
              <input type="checkbox" checked={form.isDefault} onChange={e => setForm({ ...form, isDefault: e.target.checked })} />
              Default fallback rule
            </label>
          </div>
        </div>
        {error && <div className="alert alert-error">{error}</div>}
        <div className="row">
          <button className="btn btn-primary" type="submit">{editingId ? 'Save changes' : 'Add rule'}</button>
          {editingId && <button type="button" className="btn btn-ghost" onClick={cancelEdit}>Cancel</button>}
        </div>
      </form>

      <div className="card">
        {loading ? <p className="muted">Loading…</p> : (
          <div className="table-wrap">
            <table>
              <thead><tr><th>Name</th><th>Day</th><th>Time</th><th>Price/hr</th><th></th><th></th></tr></thead>
              <tbody>
                {rules.map(r => (
                  <tr key={r.id}>
                    <td style={{ fontWeight: 600 }}>{r.name} {r.isDefault && <span className="badge badge-pending">default</span>}</td>
                    <td>{r.dayOfWeek || 'Every day'}</td>
                    <td>{r.startTime.slice(0, 5)} – {r.endTime.slice(0, 5)}</td>
                    <td>{r.pricePerHour.toFixed(3)} OMR</td>
                    <td><button className="btn btn-ghost btn-sm" onClick={() => startEdit(r)}>Edit</button></td>
                    <td><button className="btn btn-danger btn-sm" onClick={() => handleDelete(r.id)}>Delete</button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
