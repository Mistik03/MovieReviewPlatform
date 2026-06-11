import { Link } from 'react-router'
import { Star } from 'lucide-react'
import { useProfile } from '../api/queries'
import { useAuth } from '../auth/AuthContext'
import { Empty, Spinner } from '../components/Feedback'

export default function Profile() {
  const { user } = useAuth()
  const { data: profile, isLoading } = useProfile(!!user)

  if (isLoading || !profile) return <Spinner label="Loading profile" />

  return (
    <main className="mx-auto max-w-5xl px-4 pb-12 pt-24 sm:px-6">
      <p className="mb-2 text-xs uppercase tracking-[0.35em] text-amber">Member since {new Date(profile.createdAt).getFullYear()}</p>
      <h1 className="font-display text-3xl font-semibold tracking-tight sm:text-4xl">{profile.username}</h1>
      <p className="mt-2 text-sm text-muted">
        {profile.email} · {profile.role} · {profile.reviewCount} review{profile.reviewCount === 1 ? '' : 's'} ·{' '}
        {profile.ratingCount} rating{profile.ratingCount === 1 ? '' : 's'}
      </p>

      <section className="mt-12">
        <h2 className="mb-4 font-display text-xl font-medium tracking-tight sm:text-2xl">My ratings</h2>
        {profile.ratings.length === 0 ? (
          <Empty message="You have not rated anything yet." />
        ) : (
          <ul className="grid gap-3 sm:grid-cols-2">
            {profile.ratings.map((rating) => (
              <li key={rating.id}>
                <Link
                  to={`/title/${rating.titleId}`}
                  className="flex items-center gap-4 rounded-lg border border-line bg-panel/40 p-3 transition-colors hover:border-amber/40"
                >
                  {rating.titlePosterUrl && (
                    <img
                      src={rating.titlePosterUrl}
                      alt=""
                      className="h-16 w-11 rounded-sm object-cover"
                      loading="lazy"
                    />
                  )}
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-paper">{rating.titleName}</p>
                    <p className="mt-0.5 inline-flex items-center gap-1 text-sm text-amber">
                      <Star className="h-3.5 w-3.5 fill-amber" /> {rating.score}/10
                    </p>
                  </div>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="mt-12">
        <h2 className="mb-4 font-display text-xl font-medium tracking-tight sm:text-2xl">My reviews</h2>
        {profile.reviews.length === 0 ? (
          <Empty message="You have not written any reviews yet." />
        ) : (
          <ul className="space-y-4">
            {profile.reviews.map((review) => (
              <li key={review.id} className="rounded-lg border border-line bg-panel/40 p-4">
                <Link to={`/title/${review.titleId}`} className="text-sm font-medium text-amber hover:underline">
                  {review.titleName}
                </Link>
                <p className="mt-1 text-xs text-faint">
                  {new Date(review.createdAt).toLocaleDateString()}
                  {review.updatedAt && ' · edited'}
                </p>
                <p className="mt-2 line-clamp-3 whitespace-pre-line text-sm leading-relaxed text-paper/85">
                  {review.content}
                </p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </main>
  )
}
