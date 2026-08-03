import { Link } from 'react-router-dom'

export default function Layout({ children }) {
  return (
    <div>
      <header className="site-header">
        <div className="site-header-inner">
          <Link to="/" className="brand">
            <span className="brand-dot" />
            Padel Courts
          </Link>
          <nav className="site-nav">
            <Link to="/">Book a court</Link>
            <Link to="/lookup">Find my booking</Link>
          </nav>
        </div>
      </header>
      <main className="page">{children}</main>
    </div>
  )
}
