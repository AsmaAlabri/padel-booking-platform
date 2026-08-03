import { NavLink } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'

const links = [
  { to: '/admin/bookings', label: 'Bookings' },
  { to: '/admin/courts', label: 'Courts' },
  { to: '/admin/working-hours', label: 'Working Hours' },
  { to: '/admin/closures', label: 'Closures' },
  { to: '/admin/price-rules', label: 'Price Rules' },
  { to: '/admin/offers', label: 'Offers' }
]

export default function AdminLayout({ children }) {
  const { session, logout } = useAuth()

  return (
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <div style={{ fontFamily: 'var(--font-display)', fontWeight: 700, marginBottom: 20, display: 'flex', alignItems: 'center', gap: 8 }}>
          <span className="brand-dot" />
          Admin
        </div>
        {links.map(l => (
          <NavLink key={l.to} to={l.to} className={({ isActive }) => (isActive ? 'active' : '')}>
            {l.label}
          </NavLink>
        ))}
        <div style={{ marginTop: 24, paddingTop: 16, borderTop: '1px solid rgba(255,255,255,0.15)', fontSize: '0.82rem' }}>
          <div style={{ opacity: 0.7, marginBottom: 8 }}>{session?.username}</div>
          <button className="btn btn-ghost btn-sm" style={{ color: 'white', borderColor: 'rgba(255,255,255,0.3)' }} onClick={logout}>
            Log out
          </button>
        </div>
      </aside>
      <div className="admin-main">{children}</div>
    </div>
  )
}
