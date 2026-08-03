import { useEffect, useState } from 'react'
import { api } from '../../api/client.js'
import { useAuth } from '../../context/AuthContext.jsx'

const STATUSES = ['Pending', 'Confirmed', 'Cancelled', 'Completed', 'NoShow', 'Expired']
const PAYMENT_METHODS = [{ value: 'PayOnArrival', label: 'Pay on arrival' }, { value: 'Thawani', label: 'Thawani' }]

function formatTime(hms) {
  const [h, m] = hms.split(':')
  const hour = parseInt(h, 10)
  const ampm = hour >= 12 ? 'PM' : 'AM'
  const display = hour % 12 === 0 ? 12 : hour % 12
  return `${display}:${m} ${ampm}`
}

export default function BookingsPage() {
  const { session } = useAuth()
  const [bookings, setBookings] = useState([])
  const [courts, setCourts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [filters, setFilters] = useState({ date: '', status: '', courtId: '', paymentMethod: '', search: '' })

  function load() {
    setLoading(true)
    setError('')
    api.get('/admin/bookings', { params: filters, token: session.token })
      .then(setBookings)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    api.get('/admin/courts', { token: session.token }).then(setCourts).catch(() => {})
  }, []) // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(load, [filters.date, filters.status, filters.courtId, filters.paymentMethod]) // eslint-disable-line react-hooks/exhaustive-deps

  async function updateStatus(id, status) {
    try {
      await api.put(`/admin/bookings/${id}/status`, { status }, { token: session.token })
      load()
    } catch (err) {
      alert(err.message)
    }
  }

  const filtered = filters.search
    ? bookings.filter(b =>
        b.customerName.toLowerCase().includes(filters.search.toLowerCase()) ||
        b.customerPhone.includes(filters.search) ||
        b.bookingReference.toLowerCase().includes(filters.search.toLowerCase()))
    : bookings

  return (
    <div>
      <div style={{ marginBottom: 20 }}>
        <span className="section-eyebrow">Admin</span>
        <h1>Bookings</h1>
      </div>

      <div className="card">
        <div className="row" style={{ marginBottom: 16 }}>
          <div className="field" style={{ marginBottom: 0 }}>
            <label>Date</label>
            <input type="date" value={filters.date} onChange={e => setFilters({ ...filters, date: e.target.value })} />
          </div>
          <div className="field" style={{ marginBottom: 0 }}>
            <label>Status</label>
            <select value={filters.status} onChange={e => setFilters({ ...filters, status: e.target.value })}>
              <option value="">All</option>
              {STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          <div className="field" style={{ marginBottom: 0 }}>
            <label>Court</label>
            <select value={filters.courtId} onChange={e => setFilters({ ...filters, courtId: e.target.value })}>
              <option value="">All courts</option>
              {courts.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <div className="field" style={{ marginBottom: 0 }}>
            <label>Payment method</label>
            <select value={filters.paymentMethod} onChange={e => setFilters({ ...filters, paymentMethod: e.target.value })}>
              <option value="">All</option>
              {PAYMENT_METHODS.map(p => <option key={p.value} value={p.value}>{p.label}</option>)}
            </select>
          </div>
          <div className="field" style={{ marginBottom: 0, flex: 1 }}>
            <label>Search</label>
            <input placeholder="Name, phone, or reference" value={filters.search} onChange={e => setFilters({ ...filters, search: e.target.value })} />
          </div>
        </div>

        {loading && <p className="muted">Loading…</p>}
        {error && <div className="alert alert-error">{error}</div>}

        {!loading && !error && (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Reference</th>
                  <th>Customer</th>
                  <th>Date / Time</th>
                  <th>Court</th>
                  <th>Total</th>
                  <th>Payment</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map(b => (
                  <tr key={b.id}>
                    <td style={{ fontFamily: 'var(--font-display)', fontWeight: 600 }}>{b.bookingReference}</td>
                    <td>
                      <div>{b.customerName}</div>
                      <div className="muted" style={{ fontSize: '0.8rem' }}>{b.customerPhone}</div>
                    </td>
                    <td>{b.bookingDate}<br /><span className="muted">{formatTime(b.startTime)} – {formatTime(b.endTime)}</span></td>
                    <td>{b.courtName}</td>
                    <td>{b.totalPrice.toFixed(3)} OMR</td>
                    <td>{b.paymentMethod === 'Thawani' ? 'Thawani' : 'On arrival'} <br /><span className="muted">{b.paymentStatus}</span></td>
                    <td>
                      <select value={b.status} onChange={e => updateStatus(b.id, e.target.value)}>
                        {STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                      </select>
                    </td>
                  </tr>
                ))}
                {filtered.length === 0 && (
                  <tr><td colSpan={7} className="muted text-center" style={{ padding: 24 }}>No bookings match these filters.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
