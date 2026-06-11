import { Link } from 'react-router'

export default function NotFound() {
  return (
    <main className="flex min-h-svh flex-col items-center justify-center px-4 text-center">
      <p className="font-display text-7xl font-semibold text-amber">404</p>
      <h1 className="mt-3 font-display text-2xl tracking-tight">This reel is missing.</h1>
      <p className="mt-2 text-sm text-muted">The page you are looking for does not exist.</p>
      <Link
        to="/"
        className="mt-6 rounded-full border border-amber/60 px-6 py-2 text-sm text-amber transition-colors hover:bg-amber hover:text-ink"
      >
        Back to the lobby
      </Link>
    </main>
  )
}
