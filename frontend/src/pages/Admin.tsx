import { useState } from 'react'
import { Link } from 'react-router'
import { Check, Download, Plus, Trash2 } from 'lucide-react'
import { errorMessage } from '../api/client'
import {
  useCreateGenre,
  useDeleteGenre,
  useDeleteTitle,
  useGenres,
  useTitles,
  useTmdbImport,
  useTmdbSearch,
} from '../api/queries'
import { Empty, ErrorNote, Spinner } from '../components/Feedback'

type Tab = 'import' | 'titles' | 'genres'

export default function Admin() {
  const [tab, setTab] = useState<Tab>('import')

  return (
    <main className="mx-auto max-w-6xl px-4 pb-12 pt-24 sm:px-6">
      <p className="mb-2 text-xs uppercase tracking-[0.35em] text-amber">Projection booth</p>
      <h1 className="font-display text-3xl font-semibold tracking-tight sm:text-4xl">Admin</h1>

      <div className="mt-6 flex gap-2 border-b border-line">
        {(
          [
            { key: 'import', label: 'TMDB Import' },
            { key: 'titles', label: 'Titles' },
            { key: 'genres', label: 'Genres' },
          ] as const
        ).map((item) => (
          <button
            key={item.key}
            onClick={() => setTab(item.key)}
            className={`-mb-px border-b-2 px-4 py-2.5 text-sm transition-colors ${
              tab === item.key
                ? 'border-amber text-amber'
                : 'border-transparent text-muted hover:text-paper'
            }`}
          >
            {item.label}
          </button>
        ))}
      </div>

      {tab === 'import' && <ImportTab />}
      {tab === 'titles' && <TitlesTab />}
      {tab === 'genres' && <GenresTab />}
    </main>
  )
}

function ImportTab() {
  const [query, setQuery] = useState('')
  const search = useTmdbSearch(query)
  const importMutation = useTmdbImport()
  const [error, setError] = useState<string | null>(null)
  const [importingId, setImportingId] = useState<number | null>(null)

  const handleImport = async (tmdbId: number, mediaType: string) => {
    setError(null)
    setImportingId(tmdbId)
    try {
      await importMutation.mutateAsync({ tmdbId, mediaType })
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setImportingId(null)
    }
  }

  return (
    <section className="mt-6">
      <input
        value={query}
        onChange={(event) => setQuery(event.target.value)}
        placeholder="Search TMDB for movies and TV shows…"
        className="w-full max-w-xl rounded-full border border-line bg-panel px-5 py-2.5 text-sm text-paper placeholder:text-faint focus:border-amber/60 focus:outline-none"
      />
      {error && <div className="mt-3"><ErrorNote message={error} /></div>}

      {search.isLoading && <Spinner label="Searching TMDB" />}
      {search.data && search.data.length === 0 && <Empty message="No TMDB results." />}

      {search.data && search.data.length > 0 && (
        <ul className="mt-6 grid gap-3 md:grid-cols-2">
          {search.data.map((item) => (
            <li
              key={`${item.mediaType}-${item.tmdbId}`}
              className="flex gap-3 rounded-lg border border-line bg-panel/40 p-3"
            >
              <div className="h-24 w-16 shrink-0 overflow-hidden rounded-sm bg-raised">
                {item.posterUrl && (
                  <img src={item.posterUrl} alt="" loading="lazy" className="h-full w-full object-cover" />
                )}
              </div>
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium text-paper">
                  {item.name} <span className="text-faint">{item.releaseYear ?? ''}</span>
                </p>
                <p className="text-xs text-muted">
                  {item.mediaType === 'Movie' ? 'Film' : 'Series'} · TMDB {item.voteAverage.toFixed(1)}
                </p>
                {item.overview && (
                  <p className="mt-1 line-clamp-2 text-xs leading-relaxed text-muted">{item.overview}</p>
                )}
              </div>
              {item.alreadyImported ? (
                <Link
                  to={`/title/${item.titleId}`}
                  className="inline-flex h-fit items-center gap-1 self-center rounded-full border border-line px-3 py-1.5 text-xs text-muted"
                >
                  <Check className="h-3.5 w-3.5 text-amber" /> In catalog
                </Link>
              ) : (
                <button
                  onClick={() => handleImport(item.tmdbId, item.mediaType)}
                  disabled={importingId === item.tmdbId}
                  className="inline-flex h-fit items-center gap-1 self-center rounded-full bg-amber px-3 py-1.5 text-xs font-medium text-ink transition-colors enabled:hover:bg-amber-soft disabled:opacity-50"
                >
                  <Download className="h-3.5 w-3.5" />
                  {importingId === item.tmdbId ? 'Importing…' : 'Import'}
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}

function TitlesTab() {
  const [page, setPage] = useState(1)
  const titles = useTitles({ sort: 'newest', page, pageSize: 20 })
  const deleteTitle = useDeleteTitle()
  const [error, setError] = useState<string | null>(null)

  return (
    <section className="mt-6">
      {error && <div className="mb-3"><ErrorNote message={error} /></div>}
      {titles.isLoading ? (
        <Spinner label="Loading titles" />
      ) : (
        <>
          <ul className="divide-y divide-line rounded-lg border border-line">
            {titles.data?.items.map((title) => (
              <li key={title.id} className="flex items-center gap-3 p-3">
                <div className="h-14 w-10 shrink-0 overflow-hidden rounded-sm bg-raised">
                  {title.posterUrl && (
                    <img src={title.posterUrl} alt="" loading="lazy" className="h-full w-full object-cover" />
                  )}
                </div>
                <div className="min-w-0 flex-1">
                  <Link to={`/title/${title.id}`} className="truncate text-sm font-medium text-paper hover:text-amber">
                    {title.name}
                  </Link>
                  <p className="text-xs text-muted">
                    {title.releaseYear} · {title.mediaType === 'Movie' ? 'Film' : 'Series'} ·{' '}
                    {title.reviewCount} reviews · {title.ratingCount} ratings
                  </p>
                </div>
                <button
                  aria-label={`Delete ${title.name}`}
                  onClick={() => {
                    setError(null)
                    deleteTitle.mutateAsync(title.id).catch((err) => setError(errorMessage(err)))
                  }}
                  className="rounded-full border border-line p-2 text-muted transition-colors hover:border-danger/60 hover:text-danger"
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              </li>
            ))}
          </ul>
          {(titles.data?.totalPages ?? 1) > 1 && (
            <div className="mt-4 flex items-center gap-3 text-sm text-muted">
              <button
                disabled={page <= 1}
                onClick={() => setPage((value) => value - 1)}
                className="rounded-full border border-line px-4 py-1.5 enabled:hover:text-paper disabled:opacity-40"
              >
                Prev
              </button>
              Page {page} of {titles.data?.totalPages}
              <button
                disabled={page >= (titles.data?.totalPages ?? 1)}
                onClick={() => setPage((value) => value + 1)}
                className="rounded-full border border-line px-4 py-1.5 enabled:hover:text-paper disabled:opacity-40"
              >
                Next
              </button>
            </div>
          )}
        </>
      )}
      <p className="mt-3 text-xs text-faint">
        Titles that still have reviews or ratings are protected by the service layer and cannot be deleted.
      </p>
    </section>
  )
}

function GenresTab() {
  const genres = useGenres()
  const createGenre = useCreateGenre()
  const deleteGenre = useDeleteGenre()
  const [name, setName] = useState('')
  const [error, setError] = useState<string | null>(null)

  const handleCreate = async () => {
    setError(null)
    try {
      await createGenre.mutateAsync(name.trim())
      setName('')
    } catch (err) {
      setError(errorMessage(err))
    }
  }

  return (
    <section className="mt-6 max-w-2xl">
      <div className="flex gap-2">
        <input
          value={name}
          onChange={(event) => setName(event.target.value)}
          placeholder="New genre name…"
          className="flex-1 rounded-full border border-line bg-panel px-4 py-2 text-sm text-paper placeholder:text-faint focus:border-amber/60 focus:outline-none"
        />
        <button
          onClick={handleCreate}
          disabled={name.trim().length < 2 || createGenre.isPending}
          className="inline-flex items-center gap-1.5 rounded-full bg-amber px-4 py-2 text-sm font-medium text-ink transition-colors enabled:hover:bg-amber-soft disabled:opacity-40"
        >
          <Plus className="h-4 w-4" /> Add
        </button>
      </div>
      {error && <div className="mt-3"><ErrorNote message={error} /></div>}

      {genres.isLoading ? (
        <Spinner label="Loading genres" />
      ) : (
        <ul className="mt-5 divide-y divide-line rounded-lg border border-line">
          {genres.data?.map((genre) => (
            <li key={genre.id} className="flex items-center gap-3 px-4 py-2.5">
              <span className="text-sm text-paper">{genre.name}</span>
              <span className="text-xs text-faint">{genre.titleCount} titles</span>
              <button
                aria-label={`Delete genre ${genre.name}`}
                onClick={() => {
                  setError(null)
                  deleteGenre.mutateAsync(genre.id).catch((err) => setError(errorMessage(err)))
                }}
                className="ml-auto rounded-full border border-line p-1.5 text-muted transition-colors hover:border-danger/60 hover:text-danger"
              >
                <Trash2 className="h-3.5 w-3.5" />
              </button>
            </li>
          ))}
        </ul>
      )}
      <p className="mt-3 text-xs text-faint">Genres still assigned to titles cannot be deleted (service-layer rule).</p>
    </section>
  )
}
