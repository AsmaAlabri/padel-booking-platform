import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext.jsx'
import { ApiError } from '../../api/client.js'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await login(username, password)
      navigate('/admin/bookings')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Login failed.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--color-court)' }}>
      <form className="card" style={{ width: 360 }} onSubmit={handleSubmit}>
        <div className="brand" style={{ color: 'var(--color-court)', marginBottom: 20 }}>
          <span className="brand-dot" />
          Padel Courts Admin
        </div>
        <div className="field">
          <label htmlFor="u">Username</label>
          <input id="u" required value={username} onChange={e => setUsername(e.target.value)} autoFocus />
        </div>
        <div className="field">
          <label htmlFor="p">Password</label>
          <input id="p" required type="password" value={password} onChange={e => setPassword(e.target.value)} />
        </div>
        {error && <div className="alert alert-error">{error}</div>}
        <button className="btn btn-primary" style={{ width: '100%' }} disabled={loading}>
          {loading ? <span className="spinner-inline" /> : 'Log in'}
        </button>
      </form>
    </div>
  )
}
