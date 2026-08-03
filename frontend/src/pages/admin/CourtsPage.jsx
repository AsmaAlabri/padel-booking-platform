import { useEffect, useState } from 'react'
import { api } from '../../api/client.js'
import { useAuth } from '../../context/AuthContext.jsx'

const emptyForm = { name: '', description: '', isActive: true }

export default function CourtsPage() {
  const { session } = useAuth()
  const [courts, setCourts] = useState([])
  const [loading, setLoading] = useState(true)
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState(null)
  const [error, setError] = useState('')

  function load() {
    setLoading(true)
    api.get('/admin/courts', { token: session.token }).then(setCourts).finally(() => setLoading(false))
  }
  useEffect(load, []) // eslint-disable-line react-hooks/exhaustive-deps

  function startEdit(court) {
    setEditingId(court.id)
    setForm({ name: court.name, description: court.description || '', isActive: court.isActive })
  }

  function cancelEdit() {
    setEditingId(null)
    setForm(emptyForm)
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    try {
      if (editingId) {
        await api.put(`/admin/courts/${editingId}`, form, { token: session.token })
      } else {
        await api.post('/admin/courts', { name: form.name, description: form.description }, { token: session.token })
      }
      cancelEdit()
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function handleDelete(id) {
    if (!confirm('Delete this court? Courts with existing bookings cannot be deleted.')) return
    try {
      await api.del(`/admin/courts/${id}`, { token: session.token })
      load()
    } catch (err) {
      alert(err.message)
    }
  }

  return (
    <div>
      <div style={{ marginBottom: 20 }}>
        <span className="section-eyebrow">Admin</span>
        <h1>Courts</h1>
      </div>

      <form className="card" onSubmit={handleSubmit}>
        <h3 style={{ marginBottom: 16 }}>{editingId ? 'Edit court' : 'Add a court'}</h3>
        <div className="field-row">
          <div className="field">
            <label>Name</label>
            <input required value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} />
          </div>
          <div className="field">
            <label>Description</label>
            <input value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} />
          </div>
        </div>
        {editingId && (
          <div className="field">
            <label className="row" style={{ fontWeight: 400 }}>
              <input type="checkbox" checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })} />
              Active
            </label>
          </div>
        )}
        {error && <div className="alert alert-error">{error}</div>}
        <div className="row">
          <button className="btn btn-primary" type="submit">{editingId ? 'Save changes' : 'Add court'}</button>
          {editingId && <button type="button" className="btn btn-ghost" onClick={cancelEdit}>Cancel</button>}
        </div>
      </form>

      <div className="card">
        {loading ? <p className="muted">Loading…</p> : (
          <div className="table-wrap">
            <table>
              <thead><tr><th>Name</th><th>Description</th><th>Status</th><th></th></tr></thead>
              <tbody>
                {courts.map(c => (
                  <tr key={c.id}>
                    <td style={{ fontWeight: 600 }}>{c.name}</td>
                    <td className="muted">{c.description || '—'}</td>
                    <td><span className={`badge ${c.isActive ? 'badge-confirmed' : 'badge-cancelled'}`}>{c.isActive ? 'Active' : 'Inactive'}</span></td>
                    <td>
                      <div className="row">
                        <button className="btn btn-ghost btn-sm" onClick={() => startEdit(c)}>Edit</button>
                        <button className="btn btn-danger btn-sm" onClick={() => handleDelete(c.id)}>Delete</button>
                      </div>
                    </td>
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
