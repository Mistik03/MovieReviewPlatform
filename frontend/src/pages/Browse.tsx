import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router'
import { ChevronLeft, ChevronRight, Search } from 'lucide-react'
import { useGenres, useTitles } from '../api/queries'
import type { MediaType, TitleQuery } from '../api/types'
import { Empty, Spinner } from '../components/Feedback'
import PosterCard from '../components/PosterCard'

const SORTS = [
  { value: 'newest', label: 'Newest' },
  { value: 'rating', label: 'Top rated' },
  { value: 'name', label: 'A — Z' },
  { value: 'year', label: 'Release year' },
] as const

export default function Browse() {
  const [params, setParams] = useSearchParams()
  const [searchInput, setSearchInput] = useState(params.get('search') ?? '')

  const query: TitleQuery = {
    mediaType: (params.get('mediaType') as MediaType | null) ?? undefined,
    genreId: params.get('genreId') ? Number(params.get('genreId')) : undefined,
    search: params.get('search') ?? undefined,
    sort: (params.get('sort') as TitleQuery['sort']) ?? 'newest',
    page: params.get('page') ? Number(params.get('page')) : 1,
    pageSize: 24,
  }

  const titles = useTitles(query)
  const genres = useGenres()

  // Debounce typed search into the URL so back/forward and refresh keep state.
  useEffect(() => {
    const handle = setTimeout(() => {
      const current = params.get('search') ?? ''
      if (searchInput.trim() !== current) {
        update({ search: searchInput.trim() || null, page: null })
      }
    }, 350)
    return () => clearTimeout(handle)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInput])

  function update(changes: Record<string, string | null>) {
    const next = new URLSearchParams(params)
    for (const [key, value] of Object.entries(changes)) {
      if (value === null || value === '') next.delete(key)
      else next.set(key, value)
    }
    setParams(next, { preventScrollReset: true })
  }

  const mediaType = params.get('mediaType')
  const genreId = params.get('genreId')
  const page = query.page ?? 1
  const totalPages = titles.data?.totalPages ?? 1

  return (
    <main className="mx-auto max-w-7xl px-4 pb-12 pt-24 sm:px-6">
      <h1 className="font-display text-3xl font-semibold tracking-tight sm:text-4xl">Browse</h1>

      {/* Filter bar */}
      <div className="mt-6 flex flex-col gap-4">
        <div className="flex flex-wrap items-center gap-2">
          {[
            { value: null, label: 'All' },
            { value: 'Movie', label: 'Films' },
            { value: 'TvShow', label: 'Series' },
          ].map((option) => (
            <button
              key={option.label}
              onClick={() => update({ mediaType: option.value, page: null })}
              className={`rounded-full border px-4 py-1.5 text-sm transition-colors ${
                mediaType === option.value || (!mediaType && option.value === null)
                  ? 'border-amber bg-amber text-ink'
                  : 'border-line text-muted hover:border-amber/50 hover:text-paper'
              }`}
            >
              {option.label}
            </button>
          ))}

          <div className="relative ml-auto w-full sm:w-64">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-faint" />
            <input
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              placeholder="Search titles…"
              className="w-full rounded-full border border-line bg-panel py-2 pl-9 pr-4 text-sm text-paper placeholder:text-faint focus:border-amber/60 focus:outline-none"
            />
          </div>

          <select
            value={query.sort}
            onChange={(event) => update({ sort: event.target.value, page: null })}
            aria-label="Sort by"
            className="rounded-full border border-line bg-panel px-4 py-2 text-sm text-paper focus:border-amber/60 focus:outline-none"
          >
            {SORTS.map((sort) => (
              <option key={sort.value} value={sort.value}>
                {sort.label}
              </option>
            ))}
          </select>
        </div>

        {/* Genre chips */}
        <div className="scroll-row flex gap-2 overflow-x-auto pb-1">
          <button
            onClick={() => update({ genreId: null, page: null })}
            className={`shrink-0 rounded-full border px-3 py-1 text-xs transition-colors ${
              !genreId ? 'border-amber/70 text-amber' : 'border-line text-muted hover:text-paper'
            }`}
          >
            All genres
          </button>
          {genres.data?.map((genre) => (
            <button
              key={genre.id}
              onClick={() => update({ genreId: String(genre.id), page: null })}
              className={`shrink-0 rounded-full border px-3 py-1 text-xs transition-colors ${
                genreId === String(genre.id)
                  ? 'border-amber/70 text-amber'
                  : 'border-line text-muted hover:text-paper'
              }`}
            >
              {genre.name} <span className="text-faint">{genre.titleCount}</span>
            </button>
          ))}
        </div>
      </div>

      {/* Results */}
      {titles.isLoading ? (
        <Spinner label="Loading titles" />
      ) : titles.data && titles.data.items.length > 0 ? (
        <>
          <div className="mt-8 grid grid-cols-2 justify-items-center gap-x-4 gap-y-8 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6">
            {titles.data.items.map((title) => (
              <PosterCard key={title.id} title={title} />
            ))}
          </div>

          {totalPages > 1 && (
            <div className="mt-10 flex items-center justify-center gap-4 text-sm">
              <button
                disabled={page <= 1}
                onClick={() => update({ page: String(page - 1) })}
                className="inline-flex items-center gap-1 rounded-full border border-line px-4 py-1.5 text-muted transition-colors enabled:hover:border-amber/50 enabled:hover:text-paper disabled:opacity-40"
              >
                <ChevronLeft className="h-4 w-4" /> Prev
              </button>
              <span className="text-muted">
                Page <span className="text-paper">{page}</span> of {totalPages}
              </span>
              <button
                disabled={page >= totalPages}
                onClick={() => update({ page: String(page + 1) })}
                className="inline-flex items-center gap-1 rounded-full border border-line px-4 py-1.5 text-muted transition-colors enabled:hover:border-amber/50 enabled:hover:text-paper disabled:opacity-40"
              >
                Next <ChevronRight className="h-4 w-4" />
              </button>
            </div>
          )}
        </>
      ) : (
        <Empty message="No titles match those filters." />
      )}
    </main>
  )
}
