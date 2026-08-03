import { useEffect, useMemo, useState } from 'react'
import { api, ApiError } from '../../api/client.js'

function todayIso() {
  const d = new Date()
  return d.toISOString().slice(0, 10)
}

function formatTime(hms) {
  const [h, m] = hms.split(':')
  const hour = parseInt(h, 10)
  const ampm = hour >= 12 ? 'PM' : 'AM'
  const display = hour % 12 === 0 ? 12 : hour % 12
  return `${display}:${m} ${ampm}`
}

export default function BookingPage() {
  const [date, setDate] = useState(todayIso())
  const [slots, setSlots] = useState([])
  const [loadingSlots, setLoadingSlots] = useState(false)
  const [loadError, setLoadError] = useState('')

  const [selectedStart, setSelectedStart] = useState(null)
  const [duration, setDuration] = useState(1)

  const [form, setForm] = useState({ customerName: '', customerPhone: '', customerEmail: '', paymentMethod: 0 })
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState('')
  const [confirmation, setConfirmation] = useState(null)

  useEffect(() => {
    setSelectedStart(null)
    setLoadError('')
    setLoadingSlots(true)
    api.get('/availability', { params: { date } })
      .then(data => setSlots(data.slots || []))
      .catch(err => setLoadError(err.message || 'Could not load availability.'))
      .finally(() => setLoadingSlots(false))
  }, [date])

  const selectedIndices = useMemo(() => {
    if (selectedStart === null) return []
    const startIdx = slots.findIndex(s => s.startTime === selectedStart)
    if (startIdx === -1) return []
    return Array.from({ length: duration }, (_, i) => startIdx + i)
  }, [selectedStart, duration, slots])

  const selectionValid =
    selectedIndices.length === duration &&
    selectedIndices.every(i => slots[i] && slots[i].isAvailable)

  const totalEstimate = selectionValid
    ? selectedIndices.reduce((sum, i) => sum + slots[i].pricePerHour, 0)
    : 0

  function pickSlot(slot) {
    if (!slot.isAvailable) return
    setSelectedStart(slot.startTime)
    setSubmitError('')
  }

  async function handleSubmit(e) {
    e.preventDefault()
    if (!selectionValid) {
      setSubmitError('Please select a valid time range.')
      return
    }
    setSubmitting(true)
    setSubmitError('')

    try {
      const booking = await api.post('/bookings', {
        date,
        startTime: selectedStart,
        durationHours: duration,
        customerName: form.customerName,
        customerPhone: form.customerPhone,
        customerEmail: form.customerEmail || null,
        paymentMethod: Number(form.paymentMethod) === 1 ? 'Thawani' : 'PayOnArrival'
      })

      if (Number(form.paymentMethod) === 1) {
        // Thawani — redirect to hosted checkout
        const init = await api.post(`/payments/thawani/initiate/${booking.bookingReference}`)
        window.location.href = init.checkoutUrl
        return
      }

      setConfirmation(booking)
    } catch (err) {
      setSubmitError(err instanceof ApiError ? err.message : 'Something went wrong. Please try again.')
    } finally {
      setSubmitting(false)
    }
  }

  if (confirmation) {
    return (
      <div className="card" style={{ maxWidth: 520, margin: '0 auto' }}>
        <span className="section-eyebrow">Booking confirmed</span>
        <h2>You're on the court 🎾</h2>
        <p className="muted">Save this reference — you'll need it to look up or cancel your booking.</p>
        <div style={{ background: 'var(--color-sand)', borderRadius: 8, padding: '16px 20px', margin: '16px 0', fontFamily: 'var(--font-display)', fontSize: '1.4rem', fontWeight: 700, textAlign: 'center' }}>
          {confirmation.bookingReference}
        </div>
        <div className="stack">
          <div className="row-between"><span className="muted">Date</span><strong>{confirmation.bookingDate}</strong></div>
          <div className="row-between"><span className="muted">Time</span><strong>{formatTime(confirmation.startTime)} – {formatTime(confirmation.endTime)}</strong></div>
          <div className="row-between"><span className="muted">Total</span><strong>{confirmation.totalPrice.toFixed(3)} OMR</strong></div>
          <div className="row-between"><span className="muted">Payment</span><strong>{confirmation.paymentMethod === 'Thawani' ? 'Thawani (paid online)' : 'Pay on arrival'}</strong></div>
        </div>
        <button className="btn btn-primary" style={{ marginTop: 20, width: '100%' }} onClick={() => { setConfirmation(null); setSelectedStart(null) }}>
          Book another slot
        </button>
      </div>
    )
  }

  return (
    <div>
      <div style={{ marginBottom: 28 }}>
        <span className="section-eyebrow">Book a court</span>
        <h1 style={{ fontSize: '2rem' }}>Pick a date and time</h1>
        <p className="muted">Every court is the same great surface — you'll be assigned one automatically when you book.</p>
      </div>

      <div className="card">
        <div className="field" style={{ maxWidth: 240 }}>
          <label htmlFor="date">Date</label>
          <input id="date" type="date" min={todayIso()} value={date} onChange={e => setDate(e.target.value)} />
        </div>

        {loadingSlots && <p className="muted">Loading availability…</p>}
        {loadError && <div className="alert alert-error">{loadError}</div>}

        {!loadingSlots && !loadError && slots.length === 0 && (
          <p className="muted">The venue is closed on this date.</p>
        )}

        {!loadingSlots && slots.length > 0 && (
          <>
            <div className="slot-grid">
              {slots.map(slot => {
                const idx = slots.findIndex(s => s.startTime === slot.startTime)
                const isSelected = selectedIndices.includes(idx)
                return (
                  <div
                    key={slot.startTime}
                    className={`slot ${slot.isAvailable ? 'available' : 'unavailable'} ${isSelected ? 'selected' : ''}`}
                    onClick={() => pickSlot(slot)}
                  >
                    <div className="slot-time">{formatTime(slot.startTime)}</div>
                    <div className="slot-price">{slot.pricePerHour.toFixed(2)} OMR</div>
                  </div>
                )
              })}
            </div>

            <div className="field-row" style={{ marginTop: 20 }}>
              <div className="field">
                <label htmlFor="duration">Duration</label>
                <select id="duration" value={duration} onChange={e => setDuration(Number(e.target.value))}>
                  <option value={1}>1 hour</option>
                  <option value={2}>2 hours</option>
                  <option value={3}>3 hours</option>
                  <option value={4}>4 hours</option>
                </select>
              </div>
              <div className="field">
                <label>Estimated total</label>
                <div style={{ padding: '10px 0', fontFamily: 'var(--font-display)', fontWeight: 600, fontSize: '1.1rem' }}>
                  {selectionValid ? `${totalEstimate.toFixed(3)} OMR` : '—'}
                </div>
              </div>
            </div>
          </>
        )}
      </div>

      {selectedStart && (
        <form className="card" onSubmit={handleSubmit}>
          <span className="section-eyebrow">Your details</span>
          <h3 style={{ marginBottom: 16 }}>Confirm your booking</h3>

          <div className="field-row">
            <div className="field">
              <label htmlFor="name">Full name</label>
              <input id="name" required value={form.customerName} onChange={e => setForm({ ...form, customerName: e.target.value })} />
            </div>
            <div className="field">
              <label htmlFor="phone">Phone number</label>
              <input id="phone" required type="tel" placeholder="+968 9XXX XXXX" value={form.customerPhone} onChange={e => setForm({ ...form, customerPhone: e.target.value })} />
            </div>
          </div>

          <div className="field">
            <label htmlFor="email">Email (optional)</label>
            <input id="email" type="email" value={form.customerEmail} onChange={e => setForm({ ...form, customerEmail: e.target.value })} />
          </div>

          <div className="field">
            <label>Payment method</label>
            <div className="row">
              <label className="row" style={{ fontWeight: 400 }}>
                <input type="radio" name="pm" checked={Number(form.paymentMethod) === 0} onChange={() => setForm({ ...form, paymentMethod: 0 })} />
                Pay on arrival
              </label>
              <label className="row" style={{ fontWeight: 400 }}>
                <input type="radio" name="pm" checked={Number(form.paymentMethod) === 1} onChange={() => setForm({ ...form, paymentMethod: 1 })} />
                Pay online (Thawani)
              </label>
            </div>
          </div>

          {submitError && <div className="alert alert-error">{submitError}</div>}

          <button type="submit" className="btn btn-accent" disabled={submitting || !selectionValid} style={{ width: '100%' }}>
            {submitting ? <span className="spinner-inline" /> : `Confirm booking — ${selectionValid ? totalEstimate.toFixed(3) : '0.000'} OMR`}
          </button>
        </form>
      )}
    </div>
  )
}
