import React, { createContext, useContext, useState, useCallback } from 'react'
import { api } from '../api/client.js'

const AuthContext = createContext(null)

const STORAGE_KEY = 'padel_admin_session'

export function AuthProvider({ children }) {
  const [session, setSession] = useState(() => {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : null
  })

  const login = useCallback(async (username, password) => {
    const data = await api.post('/admin/auth/login', { username, password })
    const next = { token: data.token, username: data.username, role: data.role, expiresAtUtc: data.expiresAtUtc }
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next))
    setSession(next)
    return next
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(STORAGE_KEY)
    setSession(null)
  }, [])

  const isExpired = session && new Date(session.expiresAtUtc) < new Date()
  const isAuthenticated = !!session && !isExpired

  return (
    <AuthContext.Provider value={{ session: isAuthenticated ? session : null, isAuthenticated, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
