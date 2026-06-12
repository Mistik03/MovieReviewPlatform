import { Link } from 'react-router'
import { Film, Star, Tv } from 'lucide-react'
import type { TitleResponse } from '../api/types'

export default function PosterCard({ title }: { title: TitleResponse }) {
  return (
    <Link
      to={`/title/${title.id}`}
      className="group block w-36 shrink-0 sm:w-44"
    >
      <div className="relative aspect-2/3 overflow-hidden rounded-md border border-line bg-panel transition-colors duration-300 group-hover:border-amber/50">
        {title.posterUrl ? (
          <img
            src={title.posterUrl}
            alt={`${title.name} poster`}
            loading="lazy"
            className="h-full w-full object-cover transition-transform duration-500 ease-out group-hover:scale-[1.04]"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-faint">
            {title.mediaType === 'Movie' ? <Film className="h-8 w-8" /> : <Tv className="h-8 w-8" />}
          </div>
        )}
        {title.averageRating != null && (
          <span className="absolute right-1.5 top-1.5 inline-flex items-center gap-1 rounded-sm bg-ink/85 px-1.5 py-0.5 text-xs font-medium text-amber backdrop-blur-sm">
            <Star className="h-3 w-3 fill-amber" /> {title.averageRating.toFixed(1)}
          </span>
        )}
        <div className="pointer-events-none absolute inset-0 bg-linear-to-t from-ink/70 via-transparent to-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-100" />
      </div>
      <div className="mt-2 px-0.5">
        <p className="truncate text-sm font-medium text-paper group-hover:text-amber">{title.name}</p>
        <p className="text-xs text-muted">
          {title.releaseYear} · {title.mediaType === 'Movie' ? 'Film' : 'Series'}
        </p>
      </div>
    </Link>
  )
}
