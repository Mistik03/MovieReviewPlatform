import { Loader2, SearchX } from 'lucide-react'

export function Spinner({ label = 'Loading' }: { label?: string }) {
  return (
    <div className="flex items-center justify-center gap-2 py-16 text-muted" role="status">
      <Loader2 className="h-5 w-5 animate-spin text-amber" />
      <span className="text-sm">{label}…</span>
    </div>
  )
}

export function Empty({ message }: { message: string }) {
  return (
    <div className="flex flex-col items-center gap-3 py-16 text-muted">
      <SearchX className="h-8 w-8 text-faint" strokeWidth={1.5} />
      <p className="text-sm">{message}</p>
    </div>
  )
}

export function ErrorNote({ message }: { message: string }) {
  return (
    <p className="rounded-md border border-danger/40 bg-danger/10 px-3 py-2 text-sm text-danger" role="alert">
      {message}
    </p>
  )
}
