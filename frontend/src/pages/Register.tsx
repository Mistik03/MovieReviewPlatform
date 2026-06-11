import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router'
import { errorMessage } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { ErrorNote } from '../components/Feedback'

export default function Register() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [pending, setPending] = useState(false)

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)
    setPending(true)
    try {
      await register(username, email, password)
      navigate('/', { replace: true })
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setPending(false)
    }
  }

  return (
    <main className="mx-auto flex min-h-svh max-w-md flex-col justify-center px-4 py-24">
      <p className="mb-2 text-xs uppercase tracking-[0.35em] text-amber">Join the audience</p>
      <h1 className="font-display text-3xl font-semibold tracking-tight">Create account</h1>
      <form onSubmit={submit} className="mt-8 space-y-4">
        <label className="block">
          <span className="mb-1.5 block text-sm text-muted">Username</span>
          <input
            value={username}
            onChange={(event) => setUsername(event.target.value)}
            required
            minLength={3}
            maxLength={50}
            pattern="[a-zA-Z0-9_]+"
            title="Letters, digits and underscores only"
            autoComplete="username"
            className="w-full rounded-md border border-line bg-panel px-3 py-2.5 text-sm text-paper focus:border-amber/60 focus:outline-none"
          />
        </label>
        <label className="block">
          <span className="mb-1.5 block text-sm text-muted">Email</span>
          <input
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
            autoComplete="email"
            className="w-full rounded-md border border-line bg-panel px-3 py-2.5 text-sm text-paper focus:border-amber/60 focus:outline-none"
          />
        </label>
        <label className="block">
          <span className="mb-1.5 block text-sm text-muted">Password (min. 8 characters)</span>
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            minLength={8}
            maxLength={128}
            autoComplete="new-password"
            className="w-full rounded-md border border-line bg-panel px-3 py-2.5 text-sm text-paper focus:border-amber/60 focus:outline-none"
          />
        </label>
        {error && <ErrorNote message={error} />}
        <button
          type="submit"
          disabled={pending}
          className="w-full rounded-full bg-amber py-2.5 text-sm font-medium text-ink transition-colors enabled:hover:bg-amber-soft disabled:opacity-50"
        >
          {pending ? 'Creating…' : 'Create account'}
        </button>
      </form>
      <p className="mt-6 text-sm text-muted">
        Already a member?{' '}
        <Link to="/login" className="text-amber underline-offset-2 hover:underline">
          Sign in
        </Link>
      </p>
    </main>
  )
}
