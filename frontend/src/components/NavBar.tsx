import { useState } from 'react'
import { Link, NavLink, useNavigate } from 'react-router'
import { Clapperboard, LogOut, Menu, Search, Shield, User, X } from 'lucide-react'
import { useAuth } from '../auth/AuthContext'

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `text-sm tracking-wide transition-colors hover:text-paper ${isActive ? 'text-amber' : 'text-muted'}`

export default function NavBar() {
  const { user, isAdmin, logout } = useAuth()
  const [open, setOpen] = useState(false)
  const navigate = useNavigate()

  const close = () => setOpen(false)

  const handleLogout = () => {
    logout()
    close()
    navigate('/')
  }

  return (
    <header className="fixed inset-x-0 top-0 z-40 border-b border-line/60 bg-ink/80 backdrop-blur-md">
      <nav className="mx-auto flex h-14 max-w-7xl items-center gap-6 px-4 sm:px-6">
        <Link to="/" className="flex items-center gap-2.5" onClick={close}>
          <Clapperboard className="h-5 w-5 text-amber" strokeWidth={1.75} />
          <span className="font-display text-lg font-semibold tracking-tight">Marquee</span>
        </Link>

        <div className="hidden items-center gap-6 md:flex">
          <NavLink to="/browse" className={linkClass}>
            Browse
          </NavLink>
          <NavLink to="/browse?mediaType=Movie" className={() => linkClass({ isActive: false })}>
            Films
          </NavLink>
          <NavLink to="/browse?mediaType=TvShow" className={() => linkClass({ isActive: false })}>
            Series
          </NavLink>
        </div>

        <div className="ml-auto hidden items-center gap-5 md:flex">
          <Link to="/browse" aria-label="Search" className="text-muted transition-colors hover:text-paper">
            <Search className="h-4.5 w-4.5" strokeWidth={1.75} />
          </Link>
          {isAdmin && (
            <NavLink to="/admin" className={linkClass}>
              <span className="inline-flex items-center gap-1.5">
                <Shield className="h-4 w-4" strokeWidth={1.75} /> Admin
              </span>
            </NavLink>
          )}
          {user ? (
            <>
              <NavLink to="/profile" className={linkClass}>
                <span className="inline-flex items-center gap-1.5">
                  <User className="h-4 w-4" strokeWidth={1.75} /> {user.username}
                </span>
              </NavLink>
              <button
                onClick={handleLogout}
                className="inline-flex items-center gap-1.5 text-sm text-muted transition-colors hover:text-paper"
              >
                <LogOut className="h-4 w-4" strokeWidth={1.75} /> Sign out
              </button>
            </>
          ) : (
            <>
              <NavLink to="/login" className={linkClass}>
                Sign in
              </NavLink>
              <Link
                to="/register"
                className="rounded-full border border-amber/60 px-4 py-1.5 text-sm text-amber transition-colors hover:bg-amber hover:text-ink"
              >
                Join
              </Link>
            </>
          )}
        </div>

        <button
          className="ml-auto p-2 text-paper md:hidden"
          aria-label={open ? 'Close menu' : 'Open menu'}
          onClick={() => setOpen((value) => !value)}
        >
          {open ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
        </button>
      </nav>

      {open && (
        <div className="border-t border-line/60 bg-ink/95 px-6 py-4 md:hidden">
          <div className="flex flex-col gap-4">
            <NavLink to="/browse" className={linkClass} onClick={close}>
              Browse
            </NavLink>
            <NavLink to="/browse?mediaType=Movie" className={() => linkClass({ isActive: false })} onClick={close}>
              Films
            </NavLink>
            <NavLink to="/browse?mediaType=TvShow" className={() => linkClass({ isActive: false })} onClick={close}>
              Series
            </NavLink>
            {isAdmin && (
              <NavLink to="/admin" className={linkClass} onClick={close}>
                Admin
              </NavLink>
            )}
            {user ? (
              <>
                <NavLink to="/profile" className={linkClass} onClick={close}>
                  Profile — {user.username}
                </NavLink>
                <button onClick={handleLogout} className="text-left text-sm text-muted">
                  Sign out
                </button>
              </>
            ) : (
              <>
                <NavLink to="/login" className={linkClass} onClick={close}>
                  Sign in
                </NavLink>
                <NavLink to="/register" className={linkClass} onClick={close}>
                  Create account
                </NavLink>
              </>
            )}
          </div>
        </div>
      )}
    </header>
  )
}
