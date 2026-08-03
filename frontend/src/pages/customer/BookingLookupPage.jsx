import { useState } from 'react'
import { api, ApiError } from '../../api/client.js'

function formatTime(hms) {
  const [h, m] = hms.split(':')
  const hour = parseInt(h, 10)
  const ampm = hour >= 12 ? 'PM' : 'AM'
  const display = hour % 12 === 0 ? 12 : hour % 12
  return `${display}:${m} ${ampm}`
}

const statusLabel = {
  Pending: 'Pending payment',
  Confirmed: 'Confirmed',
  Cancelled: 'Cancelled',
  Completed: 'Completed',
  NoShow: 'No-show',
  Expired: 'Expired'
}

export default function BookingLookupPage() {
  const [reference, setReference] = useState('')
  const [booking, setBooking] = useState(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [cancelling, setCancelling] = useState(false)
  const [message, setMessage] = useState('')

  async function handleLookup(e) {
    e.preventDefault()
    setError('')
    setMessage('')
    setLoading(true)
    try {
      const data = await api.get(`/bookings/${reference.trim()}`)
      setBooking(data)
    } catch (err) {
      setBooking(null)
      setError(err instanceof ApiError ? err.message : 'Could not find that booking.')
    } finally {
      setLoading(false)
    }
  }

  async function handleCancel() {
    if (!booking) return
    if (!confirm('Cancel this booking? This cannot be undone.')) return
    setCancelling(true)
    setError('')
    try {
      const updated = await api.post(`/bookings/${booking.bookingReference}/cancel`)
      setBooking(updated)
      setMessage('Your booking has been cancelled.')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not cancel this booking.')
    } finally {
      setCancelling(false)
    }
  }

  const canCancel = booking && ['Pending', 'Confirmed'].includes(booking.status)

  return (
    <div>
      <div style={{ marginBottom: 28 }}>
        <span className="section-eyebrow">Find my booking</span>
        <h1 style={{ fontSize: '2rem' }}>Look up a reservation</h1>
        <p className="muted">Enter the reference code from your confirmation, e.g. PB-7F3K2Q.</p>
      </div>

      <form className="card" onSubmit={handleLookup}>
        <div className="row" style={{ alignItems: 'flex-end' }}>
          <div className="field" style={{ flex: 1, marginBottom: 0 }}>
            <label htmlFor="ref">Booking reference</label>
            <input id="ref" required value={reference} onChange={e => setReference(e.target.value.toUpperCase())} placeholder="PB-XXXXXX" />
          </div>
          <button className="btn btn-primary" disabled={loading}>
            {loading ? <span className="spinner-inline" /> : 'Find'}
          </button>
        </div>
      </form>

      {error && <div className="alert alert-error">{error}</div>}
      {message && <div className="alert alert-success">{message}</div>}

      {booking && (
        <div className="card">
          <div className="row-between" style={{ marginBottom: 16 }}>
            <h3 style={{ margin: 0 }}>{booking.bookingReference}</h3>
            <span className={`badge badge-${booking.status.toLowerCase()}`}>{statusLabel[booking.status] || booking.status}</span>
          </div>
          <div className="stack">
            <div className="row-between"><span className="muted">Name</span><strong>{booking.customerName}</strong></div>
            <div className="row-between"><span className="muted">Date</span><strong>{booking.bookingDate}</strong></div>
            <div className="row-between"><span className="muted">Time</span><strong>{formatTime(booking.startTime)} – {formatTime(booking.endTime)}</strong></div>
            <div className="row-between"><span className="muted">Total</span><strong>{booking.totalPrice.toFixed(3)} OMR</strong></div>
            <div className="row-between"><span className="muted">Payment</span><strong>{booking.paymentMethod === 'Thawani' ? 'Thawani' : 'Pay on arrival'}</strong></div>
          </div>

          {canCancel && (
            <button className="btn btn-danger" style={{ marginTop: 20 }} onClick={handleCancel} disabled={cancelling}>
              {cancelling ? <span className="spinner-inline" /> : 'Cancel booking'}
            </button>
          )}
        </div>
      )}
    </div>
  )
}
