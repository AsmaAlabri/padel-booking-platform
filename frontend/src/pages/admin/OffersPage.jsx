import { useEffect, useState } from 'react'
import { api } from '../../api/client.js'
import { useAuth } from '../../context/AuthContext.jsx'

const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
const emptyForm = { name: '', description: '', discountType: 'Percentage', discountValue: '', startDate: '', endDate: '', dayOfWeek: '', isActive: true }

export default function OffersPage() {
  const { session } = useAuth()
  const [offers, setOffers] = useState([])
  const [loading, setLoading] = useState(true)
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState(null)
  const [error, setError] = useState('')

  function load() {
    setLoading(true)
    api.get('/admin/offers', { token: session.token }).then(setOffers).finally(() => setLoading(false))
  }
  useEffect(load, []) // eslint-disable-line react-hooks/exhaustive-deps

  function startEdit(o) {
    setEditingId(o.id)
    setForm({
      name: o.name,
      description: o.description || '',
      discountType: o.discountType,
      discountValue: o.discountValue,
      startDate: o.startDate,
      endDate: o.endDate,
      dayOfWeek: o.dayOfWeek ?? '',
      isActive: o.isActive
    })
  }

  function cancelEdit() { setEditingId(null); setForm(emptyForm) }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    const payload = {
      name: form.name,
      description: form.description || null,
      discountType: form.discountType,
      discountValue: Number(form.discountValue),
      startDate: form.startDate,
      endDate: form.endDate,
      dayOfWeek: form.dayOfWeek === '' ? null : form.dayOfWeek
    }
    try {
      if (editingId) {
        await api.put(`/admin/offers/${editingId}`, { ...payload, isActive: form.isActive }, { token: session.token })
      } else {
        await api.post('/admin/offers', payload, { token: session.token })
      }
      cancelEdit()
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  async function handleDelete(id) {
    if (!confirm('Delete this offer?')) return
    try {
      await api.del(`/admin/offers/${id}`, { token: session.token })
      load()
    } catch (err) {
      alert(err.message)
    }
  }

  return (
    <div>
      <div style={{ marginBottom: 20 }}>
        <span className="section-eyebrow">Admin</span>
        <h1>Offers</h1>
        <p className="muted">Discounts are applied automatically at checkout when a booking's date matches.</p>
      </div>

      <form className="card" onSubmit={handleSubmit}>
        <h3 style={{ marginBottom: 16 }}>{editingId ? 'Edit offer' : 'Add an offer'}</h3>
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
        <div className="field">
          <label>Description (optional)</label>
          <input value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} />
        </div>
        <div className="field-row">
          <div className="field">
            <label>Discount type</label>
            <select value={form.discountType} onChange={e => setForm({ ...form, discountType: e.target.value })}>
              <option value="Percentage">Percentage</option>
              <option value="FixedAmount">Fixed amount (OMR)</option>
            </select>
          </div>
          <div className="field">
            <label>Discount value</label>
            <input required type="number" step="0.001" min="0" value={form.discountValue} onChange={e => setForm({ ...form, discountValue: e.target.value })} />
          </div>
        </div>
        <div className="field-row">
          <div className="field">
            <label>Start date</label>
            <input required type="date" value={form.startDate} onChange={e => setForm({ ...form, startDate: e.target.value })} />
          </div>
          <div className="field">
            <label>End date</label>
            <input required type="date" value={form.endDate} onChange={e => setForm({ ...form, endDate: e.target.value })} />
          </div>
        </div>
        {error && <div className="alert alert-error">{error}</div>}
        <div className="row">
          <button className="btn btn-primary" type="submit">{editingId ? 'Save changes' : 'Add offer'}</button>
          {editingId && <button type="button" className="btn btn-ghost" onClick={cancelEdit}>Cancel</button>}
        </div>
      </form>

      <div className="card">
        {loading ? <p className="muted">Loading…</p> : (
          <div className="table-wrap">
            <table>
              <thead><tr><th>Name</th><th>Discount</th><th>Dates</th><th>Day</th><th></th><th></th></tr></thead>
              <tbody>
                {offers.map(o => (
                  <tr key={o.id}>
                    <td style={{ fontWeight: 600 }}>{o.name}</td>
                    <td>{o.discountType === 'Percentage' ? `${o.discountValue}%` : `${o.discountValue.toFixed(3)} OMR`}</td>
                    <td>{o.startDate} – {o.endDate}</td>
                    <td>{o.dayOfWeek || 'Every day'}</td>
                    <td><button className="btn btn-ghost btn-sm" onClick={() => startEdit(o)}>Edit</button></td>
                    <td><button className="btn btn-danger btn-sm" onClick={() => handleDelete(o.id)}>Delete</button></td>
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
