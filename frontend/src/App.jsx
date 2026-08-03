import { Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout.jsx'
import AdminLayout from './components/AdminLayout.jsx'
import ProtectedRoute from './components/ProtectedRoute.jsx'

import BookingPage from './pages/customer/BookingPage.jsx'
import BookingLookupPage from './pages/customer/BookingLookupPage.jsx'
import PaymentCallbackPage from './pages/customer/PaymentCallbackPage.jsx'

import LoginPage from './pages/admin/LoginPage.jsx'
import BookingsPage from './pages/admin/BookingsPage.jsx'
import CourtsPage from './pages/admin/CourtsPage.jsx'
import WorkingHoursPage from './pages/admin/WorkingHoursPage.jsx'
import ClosuresPage from './pages/admin/ClosuresPage.jsx'
import PriceRulesPage from './pages/admin/PriceRulesPage.jsx'
import OffersPage from './pages/admin/OffersPage.jsx'

export default function App() {
  return (
    <Routes>
      {/* Customer-facing */}
      <Route path="/" element={<Layout><BookingPage /></Layout>} />
      <Route path="/lookup" element={<Layout><BookingLookupPage /></Layout>} />
      <Route path="/payment/callback" element={<Layout><PaymentCallbackPage /></Layout>} />

      {/* Admin */}
      <Route path="/admin/login" element={<LoginPage />} />
      <Route path="/admin/bookings" element={<ProtectedRoute><AdminLayout><BookingsPage /></AdminLayout></ProtectedRoute>} />
      <Route path="/admin/courts" element={<ProtectedRoute><AdminLayout><CourtsPage /></AdminLayout></ProtectedRoute>} />
      <Route path="/admin/working-hours" element={<ProtectedRoute><AdminLayout><WorkingHoursPage /></AdminLayout></ProtectedRoute>} />
      <Route path="/admin/closures" element={<ProtectedRoute><AdminLayout><ClosuresPage /></AdminLayout></ProtectedRoute>} />
      <Route path="/admin/price-rules" element={<ProtectedRoute><AdminLayout><PriceRulesPage /></AdminLayout></ProtectedRoute>} />
      <Route path="/admin/offers" element={<ProtectedRoute><AdminLayout><OffersPage /></AdminLayout></ProtectedRoute>} />
      <Route path="/admin" element={<Navigate to="/admin/bookings" replace />} />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
