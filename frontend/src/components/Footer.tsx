export default function Footer() {
  return (
    <footer className="mt-24 border-t border-line/60">
      <div className="mx-auto flex max-w-7xl flex-col gap-2 px-4 py-8 text-xs text-faint sm:px-6 md:flex-row md:items-center md:justify-between">
        <p>
          Marquee — Service Oriented Architecture final project, Arb Xhelili, SEEU{' '}
          {new Date().getFullYear()}.
        </p>
        <p>
          Film and TV metadata and images supplied by{' '}
          <a
            href="https://www.themoviedb.org/"
            target="_blank"
            rel="noreferrer"
            className="text-muted underline-offset-2 hover:text-amber hover:underline"
          >
            TMDB
          </a>
          . This product uses the TMDB API but is not endorsed or certified by TMDB.
        </p>
      </div>
    </footer>
  )
}
