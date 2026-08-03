import { useEffect, useState } from 'react'
import { api } from '../../api/client.js'
import { useAuth } from '../../context/AuthContext.jsx'

const DAY_ORDER = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']

export default function WorkingHoursPage() {
  const { session } = useAuth()
  const [hours, setHours] = useState([])
  const [loading, setLoading] = useState(true)
  const [savingDay, setSavingDay] = useState(null)
  const [message, setMessage] = useState('')

  function load() {
    setLoading(true)
    api.get('/admin/working-hours', { token: session.token })
      .then(data => setHours([...data].sort((a, b) => DAY_ORDER.indexOf(a.dayOfWeek) - DAY_ORDER.indexOf(b.dayOfWeek))))
      .finally(() => setLoading(false))
  }

  useEffect(load, []) // eslint-disable-line react-hooks/exhaustive-deps

  function update(dayIndex, patch) {
    setHours(hours.map((h, i) => (i === dayIndex ? { ...h, ...patch } : h)))
  }

  function normalizeTime(t) {
    if (!t) return t
    return t.length === 5 ? `${t}:00` : t
  }

  async function save(day) {
    setSavingDay(day.dayOfWeek)
    setMessage('')
    try {
      await api.put(`/admin/working-hours/${day.dayOfWeek}`, {
        openTime: normalizeTime(day.openTime),
        closeTime: normalizeTime(day.closeTime),
        isClosed: day.isClosed
      }, { token: session.token })
      setMessage(`${day.dayOfWeek} hours saved.`)
    } catch (err) {
      alert(err.message)
    } finally {
      setSavingDay(null)
    }
  }

  return (
    <div>
      <div style={{ marginBottom: 20 }}>
        <span className="section-eyebrow">Admin</span>
        <h1>Working hours</h1>
        <p className="muted">Set opening and closing times for each day of the week.</p>
      </div>

      <div className="card">
        {loading && <p className="muted">Loading…</p>}
        {message && <div className="alert alert-success">{message}</div>}
        {!loading && (
          <div className="table-wrap">
            <table>
              <thead>
                <tr><th>Day</th><th>Open</th><th>Close</th><th>Closed</th><th></th></tr>
              </thead>
              <tbody>
                {hours.map((day, i) => (
                  <tr key={day.dayOfWeek}>
                    <td style={{ fontWeight: 600 }}>{day.dayOfWeek}</td>
                    <td><input type="time" value={day.openTime} disabled={day.isClosed} onChange={e => update(i, { openTime: e.target.value })} /></td>
                    <td><input type="time" value={day.closeTime} disabled={day.isClosed} onChange={e => update(i, { closeTime: e.target.value })} /></td>
                    <td><input type="checkbox" checked={day.isClosed} onChange={e => update(i, { isClosed: e.target.checked })} /></td>
                    <td>
                      <button className="btn btn-primary btn-sm" onClick={() => save(day)} disabled={savingDay === day.dayOfWeek}>
                        {savingDay === day.dayOfWeek ? <span className="spinner-inline" /> : 'Save'}
                      </button>
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
