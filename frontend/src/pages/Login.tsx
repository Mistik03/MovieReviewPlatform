import { useState, type FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import { errorMessage } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { ErrorNote } from '../components/Feedback'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [usernameOrEmail, setUsernameOrEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [pending, setPending] = useState(false)

  const from = (location.state as { from?: string } | null)?.from ?? '/'

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)
    setPending(true)
    try {
      await login(usernameOrEmail, password)
      navigate(from, { replace: true })
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setPending(false)
    }
  }

  return (
    <main className="mx-auto flex min-h-svh max-w-md flex-col justify-center px-4 py-24">
      <p className="mb-2 text-xs uppercase tracking-[0.35em] text-amber">Welcome back</p>
      <h1 className="font-display text-3xl font-semibold tracking-tight">Sign in</h1>
      <form onSubmit={submit} className="mt-8 space-y-4">
        <label className="block">
          <span className="mb-1.5 block text-sm text-muted">Username or email</span>
          <input
            value={usernameOrEmail}
            onChange={(event) => setUsernameOrEmail(event.target.value)}
            required
            autoComplete="username"
            className="w-full rounded-md border border-line bg-panel px-3 py-2.5 text-sm text-paper focus:border-amber/60 focus:outline-none"
          />
        </label>
        <label className="block">
          <span className="mb-1.5 block text-sm text-muted">Password</span>
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            autoComplete="current-password"
            className="w-full rounded-md border border-line bg-panel px-3 py-2.5 text-sm text-paper focus:border-amber/60 focus:outline-none"
          />
        </label>
        {error && <ErrorNote message={error} />}
        <button
          type="submit"
          disabled={pending}
          className="w-full rounded-full bg-amber py-2.5 text-sm font-medium text-ink transition-colors enabled:hover:bg-amber-soft disabled:opacity-50"
        >
          {pending ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
      <p className="mt-6 text-sm text-muted">
        No account yet?{' '}
        <Link to="/register" className="text-amber underline-offset-2 hover:underline">
          Create one
        </Link>
      </p>
    </main>
  )
}
