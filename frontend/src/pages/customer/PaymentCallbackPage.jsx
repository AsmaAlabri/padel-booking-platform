import { useEffect, useState } from 'react'
import { useSearchParams, Link } from 'react-router-dom'
import { api } from '../../api/client.js'

export default function PaymentCallbackPage() {
  const [params] = useSearchParams()
  const reference = params.get('reference')
  const [status, setStatus] = useState('checking')
  const [result, setResult] = useState(null)

  useEffect(() => {
    if (!reference) {
      setStatus('error')
      return
    }
    api.post(`/payments/thawani/callback?bookingReference=${encodeURIComponent(reference)}`)
      .then(data => {
        setResult(data)
        setStatus('done')
      })
      .catch(() => setStatus('error'))
  }, [reference])

  return (
    <div className="card" style={{ maxWidth: 480, margin: '0 auto', textAlign: 'center' }}>
      {status === 'checking' && (
        <>
          <span className="spinner-inline" style={{ borderTopColor: 'var(--color-court)', width: 24, height: 24, marginBottom: 16 }} />
          <h2>Confirming your payment…</h2>
          <p className="muted">Please don't close this page.</p>
        </>
      )}

      {status === 'done' && result?.bookingStatus === 'Confirmed' && (
        <>
          <h2>Payment successful 🎉</h2>
          <p className="muted">Your booking is confirmed.</p>
          <div style={{ background: 'var(--color-sand)', borderRadius: 8, padding: '14px 20px', margin: '16px 0', fontFamily: 'var(--font-display)', fontWeight: 700 }}>
            {result.bookingReference}
          </div>
        </>
      )}

      {status === 'done' && result?.bookingStatus !== 'Confirmed' && (
        <>
          <h2>Payment not completed</h2>
          <p className="muted">Your booking ({result?.bookingReference}) is currently: {result?.bookingStatus}.</p>
        </>
      )}

      {status === 'error' && (
        <>
          <h2>Something went wrong</h2>
          <p className="muted">We couldn't confirm your payment status. Please look up your booking to check.</p>
        </>
      )}

      <Link to="/lookup" className="btn btn-primary" style={{ marginTop: 16 }}>Look up my booking</Link>
    </div>
  )
}
