import { useRef, useState } from 'react'
import { Link, useParams } from 'react-router'
import { useGSAP } from '@gsap/react'
import gsap from 'gsap'
import { CalendarDays, Clock3, Megaphone, Pencil, Star, Trash2, User as UserIcon } from 'lucide-react'
import { errorMessage } from '../api/client'
import {
  useCreateRating,
  useCreateReview,
  useDeleteRating,
  useDeleteReview,
  useRatings,
  useReviews,
  useTitle,
  useUpdateRating,
  useUpdateReview,
} from '../api/queries'
import { useAuth } from '../auth/AuthContext'
import { Empty, ErrorNote, Spinner } from '../components/Feedback'
import RatingBar from '../components/RatingBar'

export default function TitleDetail() {
  const { id } = useParams()
  const titleId = Number(id)
  const { data: title, isLoading } = useTitle(titleId)
  const pageRef = useRef<HTMLDivElement>(null)

  useGSAP(
    () => {
      const matchMedia = gsap.matchMedia()
      matchMedia.add('(prefers-reduced-motion: no-preference)', () => {
        gsap.to('[data-reveal]', {
          opacity: 1,
          y: 0,
          duration: 0.6,
          ease: 'power2.out',
          stagger: 0.08,
          startAt: { y: 16 },
          delay: 0.05,
        })
      })
      matchMedia.add('(prefers-reduced-motion: reduce)', () => {
        gsap.set('[data-reveal]', { opacity: 1 })
      })
    },
    { scope: pageRef, dependencies: [title?.id] },
  )

  if (isLoading) return <Spinner label="Loading title" />
  if (!title)
    return (
      <main className="pt-24">
        <Empty message="This title does not exist." />
      </main>
    )

  return (
    <main ref={pageRef}>
      {/* Backdrop hero */}
      <section className="relative min-h-[46svh] overflow-hidden sm:min-h-[58svh]">
        {title.backdropUrl && (
          <img
            src={title.backdropUrl}
            alt=""
            aria-hidden
            className="absolute inset-0 h-full w-full object-cover object-top opacity-50"
          />
        )}
        <div className="vignette absolute inset-0" />
      </section>

      {/* Poster + meta, overlapping the hero */}
      <section className="relative mx-auto -mt-36 max-w-7xl px-4 sm:-mt-48 sm:px-6">
        <div className="flex flex-col gap-8 md:flex-row">
          <div data-reveal className="w-40 shrink-0 sm:w-56">
            {title.posterUrl ? (
              <img
                src={title.posterUrl}
                alt={`${title.name} poster`}
                className="aspect-2/3 w-full rounded-lg border border-line object-cover shadow-2xl shadow-ink"
              />
            ) : (
              <div className="aspect-2/3 w-full rounded-lg border border-line bg-panel" />
            )}
          </div>

          <div className="max-w-2xl md:pt-24">
            <p data-reveal className="mb-2 text-xs uppercase tracking-[0.3em] text-amber opacity-0">
              {title.mediaType === 'Movie' ? 'Film' : 'Series'}
            </p>
            <h1
              data-reveal
              className="font-display text-3xl font-semibold leading-tight tracking-tight text-paper opacity-0 sm:text-5xl"
            >
              {title.name}
            </h1>

            <div data-reveal className="mt-4 flex flex-wrap items-center gap-x-5 gap-y-2 text-sm text-muted opacity-0">
              <span className="inline-flex items-center gap-1.5">
                <CalendarDays className="h-4 w-4" /> {title.releaseYear}
              </span>
              {title.runtimeMinutes && (
                <span className="inline-flex items-center gap-1.5">
                  <Clock3 className="h-4 w-4" /> {title.runtimeMinutes} min
                </span>
              )}
              {title.director && (
                <span className="inline-flex items-center gap-1.5">
                  <Megaphone className="h-4 w-4" /> {title.director}
                </span>
              )}
              <span className="inline-flex items-center gap-1.5 text-amber">
                <Star className="h-4 w-4 fill-amber" />
                {title.averageRating != null ? `${title.averageRating.toFixed(1)} / 10` : 'Unrated'}
                <span className="text-faint">({title.ratingCount})</span>
              </span>
            </div>

            <div data-reveal className="mt-4 flex flex-wrap gap-2 opacity-0">
              {title.genres.map((genre) => (
                <Link
                  key={genre.id}
                  to={`/browse?genreId=${genre.id}`}
                  className="rounded-full border border-line px-3 py-1 text-xs text-muted transition-colors hover:border-amber/50 hover:text-amber"
                >
                  {genre.name}
                </Link>
              ))}
            </div>

            {title.description && (
              <p data-reveal className="mt-6 leading-relaxed text-paper/85 opacity-0">
                {title.description}
              </p>
            )}
          </div>
        </div>

        {/* Cast */}
        {title.cast.length > 0 && (
          <section className="mt-14">
            <h2 className="mb-4 font-display text-xl font-medium tracking-tight sm:text-2xl">Cast</h2>
            <div className="scroll-row flex gap-4 overflow-x-auto pb-2">
              {title.cast.map((member) => (
                <figure key={member.personId} className="w-28 shrink-0 sm:w-32">
                  <div className="aspect-3/4 overflow-hidden rounded-md border border-line bg-panel">
                    {member.profileImageUrl ? (
                      <img
                        src={member.profileImageUrl}
                        alt={member.name}
                        loading="lazy"
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <div className="flex h-full w-full items-center justify-center text-faint">
                        <UserIcon className="h-8 w-8" strokeWidth={1.25} />
                      </div>
                    )}
                  </div>
                  <figcaption className="mt-2">
                    <p className="truncate text-sm font-medium text-paper">{member.name}</p>
                    {member.characterName && (
                      <p className="truncate text-xs text-muted">{member.characterName}</p>
                    )}
                  </figcaption>
                </figure>
              ))}
            </div>
          </section>
        )}

        <RatingSection titleId={titleId} />
        <ReviewSection titleId={titleId} />
      </section>
    </main>
  )
}

function RatingSection({ titleId }: { titleId: number }) {
  const { user } = useAuth()
  const { data: ratings } = useRatings(titleId)
  const create = useCreateRating(titleId)
  const update = useUpdateRating(titleId)
  const remove = useDeleteRating(titleId)
  const [error, setError] = useState<string | null>(null)

  const mine = ratings?.find((rating) => rating.userId === user?.userId) ?? null

  const handleChange = (score: number) => {
    setError(null)
    const action = mine ? update.mutateAsync({ id: mine.id, score }) : create.mutateAsync(score)
    action.catch((err) => setError(errorMessage(err)))
  }

  return (
    <section className="mt-14 rounded-lg border border-line bg-panel/60 p-5 sm:p-6">
      <h2 className="font-display text-xl font-medium tracking-tight">Your score</h2>
      {user ? (
        <div className="mt-4 flex flex-wrap items-center gap-4">
          <RatingBar
            value={mine?.score ?? null}
            onChange={handleChange}
            disabled={create.isPending || update.isPending}
          />
          {mine && (
            <button
              onClick={() => remove.mutateAsync(mine.id).catch((err) => setError(errorMessage(err)))}
              className="inline-flex items-center gap-1.5 text-xs text-muted transition-colors hover:text-danger"
            >
              <Trash2 className="h-3.5 w-3.5" /> Remove my score
            </button>
          )}
        </div>
      ) : (
        <p className="mt-3 text-sm text-muted">
          <Link to="/login" className="text-amber underline-offset-2 hover:underline">
            Sign in
          </Link>{' '}
          to rate this title.
        </p>
      )}
      {error && <div className="mt-3"><ErrorNote message={error} /></div>}
    </section>
  )
}

function ReviewSection({ titleId }: { titleId: number }) {
  const { user, isAdmin } = useAuth()
  const { data: reviews, isLoading } = useReviews(titleId)
  const create = useCreateReview(titleId)
  const update = useUpdateReview(titleId)
  const remove = useDeleteReview(titleId)

  const [draft, setDraft] = useState('')
  const [editingId, setEditingId] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)

  const mine = reviews?.find((review) => review.userId === user?.userId)
  const isEditing = editingId !== null

  const submit = async () => {
    setError(null)
    try {
      if (isEditing) {
        await update.mutateAsync({ id: editingId, content: draft })
        setEditingId(null)
      } else {
        await create.mutateAsync(draft)
      }
      setDraft('')
    } catch (err) {
      setError(errorMessage(err))
    }
  }

  return (
    <section className="mt-10">
      <h2 className="mb-4 font-display text-xl font-medium tracking-tight sm:text-2xl">
        Reviews <span className="text-sm text-faint">{reviews?.length ?? 0}</span>
      </h2>

      {user && (!mine || isEditing) && (
        <div className="mb-8 rounded-lg border border-line bg-panel/60 p-4">
          <textarea
            value={draft}
            onChange={(event) => setDraft(event.target.value)}
            rows={4}
            minLength={10}
            maxLength={5000}
            placeholder={isEditing ? 'Edit your review…' : 'What did you think? (at least 10 characters)'}
            className="w-full resize-y rounded-md border border-line bg-ink p-3 text-sm text-paper placeholder:text-faint focus:border-amber/60 focus:outline-none"
          />
          <div className="mt-3 flex items-center gap-3">
            <button
              onClick={submit}
              disabled={draft.trim().length < 10 || create.isPending || update.isPending}
              className="rounded-full bg-amber px-5 py-2 text-sm font-medium text-ink transition-colors enabled:hover:bg-amber-soft disabled:opacity-40"
            >
              {isEditing ? 'Save changes' : 'Post review'}
            </button>
            {isEditing && (
              <button
                onClick={() => {
                  setEditingId(null)
                  setDraft('')
                }}
                className="text-sm text-muted hover:text-paper"
              >
                Cancel
              </button>
            )}
            {error && <ErrorNote message={error} />}
          </div>
        </div>
      )}

      {!user && (
        <p className="mb-8 text-sm text-muted">
          <Link to="/login" className="text-amber underline-offset-2 hover:underline">
            Sign in
          </Link>{' '}
          to write a review.
        </p>
      )}

      {isLoading ? (
        <Spinner label="Loading reviews" />
      ) : reviews && reviews.length > 0 ? (
        <ul className="space-y-4">
          {reviews.map((review) => {
            const canManage = user && (review.userId === user.userId || isAdmin)
            return (
              <li key={review.id} className="rounded-lg border border-line bg-panel/40 p-4 sm:p-5">
                <div className="mb-2 flex items-center gap-3">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full border border-line bg-raised font-display text-sm text-amber">
                    {review.username.slice(0, 1).toUpperCase()}
                  </span>
                  <div>
                    <p className="text-sm font-medium text-paper">{review.username}</p>
                    <p className="text-xs text-faint">
                      {new Date(review.createdAt).toLocaleDateString()}
                      {review.updatedAt && ' · edited'}
                    </p>
                  </div>
                  {canManage && (
                    <div className="ml-auto flex gap-2">
                      {review.userId === user?.userId && (
                        <button
                          aria-label="Edit review"
                          onClick={() => {
                            setEditingId(review.id)
                            setDraft(review.content)
                            window.scrollTo({ top: window.scrollY - 1, behavior: 'smooth' })
                          }}
                          className="rounded-full border border-line p-1.5 text-muted transition-colors hover:border-amber/50 hover:text-amber"
                        >
                          <Pencil className="h-3.5 w-3.5" />
                        </button>
                      )}
                      <button
                        aria-label="Delete review"
                        onClick={() =>
                          remove.mutateAsync(review.id).catch((err) => setError(errorMessage(err)))
                        }
                        className="rounded-full border border-line p-1.5 text-muted transition-colors hover:border-danger/60 hover:text-danger"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </button>
                    </div>
                  )}
                </div>
                <p className="whitespace-pre-line text-sm leading-relaxed text-paper/85">
                  {review.content}
                </p>
              </li>
            )
          })}
        </ul>
      ) : (
        <Empty message="No reviews yet — be the first." />
      )}
    </section>
  )
}
