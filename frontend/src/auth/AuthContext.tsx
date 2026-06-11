import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react'
import { TOKEN_KEY, USER_KEY } from '../api/client'
import { loginRequest, registerRequest } from '../api/queries'
import type { AuthResponse, Role } from '../api/types'

export interface SessionUser {
  userId: number
  username: string
  email: string
  role: Role
  expiresAtUtc: string
}

interface AuthContextValue {
  user: SessionUser | null
  isAdmin: boolean
  login: (usernameOrEmail: string, password: string) => Promise<void>
  register: (username: string, email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function readStoredUser(): SessionUser | null {
  try {
    const raw = localStorage.getItem(USER_KEY)
    if (!raw || !localStorage.getItem(TOKEN_KEY)) return null
    const user = JSON.parse(raw) as SessionUser
    // Treat expired tokens as signed out.
    if (new Date(user.expiresAtUtc).getTime() < Date.now()) return null
    return user
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<SessionUser | null>(readStoredUser)

  const storeSession = useCallback((auth: AuthResponse) => {
    const session: SessionUser = {
      userId: auth.userId,
      username: auth.username,
      email: auth.email,
      role: auth.role,
      expiresAtUtc: auth.expiresAtUtc,
    }
    localStorage.setItem(TOKEN_KEY, auth.token)
    localStorage.setItem(USER_KEY, JSON.stringify(session))
    setUser(session)
  }, [])

  const login = useCallback(
    async (usernameOrEmail: string, password: string) => {
      storeSession(await loginRequest(usernameOrEmail, password))
    },
    [storeSession],
  )

  const register = useCallback(
    async (username: string, email: string, password: string) => {
      storeSession(await registerRequest(username, email, password))
    },
    [storeSession],
  )

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    setUser(null)
  }, [])

  const value = useMemo(
    () => ({ user, isAdmin: user?.role === 'Admin', login, register, logout }),
    [user, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside AuthProvider')
  return context
}
