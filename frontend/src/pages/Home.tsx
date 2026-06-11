import { lazy, Suspense, useMemo, useRef } from 'react'
import { Link } from 'react-router'
import { useGSAP } from '@gsap/react'
import gsap from 'gsap'
import { ArrowRight, Star } from 'lucide-react'
import { useTitles } from '../api/queries'
import { Spinner } from '../components/Feedback'
import TitleRow from '../components/TitleRow'

const HeroParticles = lazy(() => import('../components/HeroParticles'))

export default function Home() {
  const topRated = useTitles({ sort: 'rating', pageSize: 18 })
  const newest = useTitles({ sort: 'newest', pageSize: 18 })
  const films = useTitles({ mediaType: 'Movie', sort: 'rating', pageSize: 18 })
  const series = useTitles({ mediaType: 'TvShow', sort: 'rating', pageSize: 18 })

  const featured = topRated.data?.items[0]
  const heroRef = useRef<HTMLDivElement>(null)

  const allowMotion = useMemo(
    () => window.matchMedia('(prefers-reduced-motion: no-preference)').matches,
    [],
  )

  useGSAP(
    () => {
      if (!allowMotion) {
        gsap.set('[data-hero]', { opacity: 1 })
        return
      }
      gsap.to('[data-hero]', {
        opacity: 1,
        y: 0,
        duration: 0.9,
        ease: 'power3.out',
        stagger: 0.12,
        startAt: { y: 26 },
        delay: 0.15,
      })
    },
    { scope: heroRef, dependencies: [featured?.id] },
  )

  return (
    <main>
      {/* Letterboxed hero */}
      <section ref={heroRef} className="relative flex min-h-[82svh] items-end overflow-hidden">
        {featured?.backdropUrl && (
          <img
            src={featured.backdropUrl}
            alt=""
            aria-hidden
            className="absolute inset-0 h-full w-full object-cover opacity-45"
          />
        )}
        <div className="vignette absolute inset-0" />
        {allowMotion && (
          <Suspense fallback={null}>
            <div className="absolute inset-0">
              <HeroParticles />
            </div>
          </Suspense>
        )}

        <div className="relative mx-auto w-full max-w-7xl px-4 pb-16 pt-32 sm:px-6">
          <p data-hero className="mb-3 text-xs uppercase tracking-[0.35em] text-amber opacity-0">
            Now showing
          </p>
          <h1
            data-hero
            className="max-w-3xl font-display text-4xl font-semibold leading-[1.05] tracking-tight text-paper opacity-0 sm:text-6xl md:text-7xl"
          >
            Every film deserves a verdict.
          </h1>
          <p data-hero className="mt-5 max-w-xl text-base leading-relaxed text-muted opacity-0 sm:text-lg">
            Browse a catalog of real films and series, read what others thought, and leave your own
            mark — one review and one score, per title, per person.
          </p>
          <div data-hero className="mt-8 flex flex-wrap items-center gap-4 opacity-0">
            <Link
              to="/browse"
              className="group inline-flex items-center gap-2 rounded-full bg-amber px-6 py-2.5 text-sm font-medium text-ink transition-colors hover:bg-amber-soft"
            >
              Browse the catalog
              <ArrowRight className="h-4 w-4 transition-transform group-hover:translate-x-0.5" />
            </Link>
            {featured && (
              <Link
                to={`/title/${featured.id}`}
                className="inline-flex items-center gap-2 text-sm text-muted transition-colors hover:text-paper"
              >
                <Star className="h-4 w-4 fill-amber text-amber" />
                Featured: {featured.name} ({featured.averageRating?.toFixed(1) ?? '—'})
              </Link>
            )}
          </div>
        </div>
      </section>

      {topRated.isLoading ? (
        <Spinner label="Loading the catalog" />
      ) : (
        <>
          <TitleRow heading="Top rated" titles={topRated.data?.items ?? []} />
          <TitleRow heading="Recently added" titles={newest.data?.items ?? []} />
          <TitleRow heading="Films" titles={films.data?.items ?? []} />
          <TitleRow heading="Series" titles={series.data?.items ?? []} />
        </>
      )}
    </main>
  )
}
