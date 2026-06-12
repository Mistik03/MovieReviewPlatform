import { useRef } from 'react'
import { useGSAP } from '@gsap/react'
import gsap from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import type { TitleResponse } from '../api/types'
import PosterCard from './PosterCard'

gsap.registerPlugin(ScrollTrigger)

interface TitleRowProps {
  heading: string
  titles: TitleResponse[]
}

/** A horizontal poster scroller whose cards stagger in the first time they scroll into view. */
export default function TitleRow({ heading, titles }: TitleRowProps) {
  const sectionRef = useRef<HTMLElement>(null)
  const rowRef = useRef<HTMLDivElement>(null)

  useGSAP(
    () => {
      // Animate the row's own cards. They are visible by default (progressive
      // enhancement); we only hide-then-reveal them when motion is allowed.
      const cards = rowRef.current ? gsap.utils.toArray<HTMLElement>(rowRef.current.children) : []
      if (cards.length === 0) return

      const matchMedia = gsap.matchMedia()
      matchMedia.add('(prefers-reduced-motion: no-preference)', () => {
        gsap.set(cards, { opacity: 0, y: 18 })
        gsap.to(cards, {
          opacity: 1,
          y: 0,
          duration: 0.55,
          ease: 'power2.out',
          stagger: 0.06,
          scrollTrigger: { trigger: sectionRef.current, start: 'top 88%', once: true },
        })
      })
    },
    { scope: sectionRef, dependencies: [titles.length] },
  )

  const scrollBy = (direction: 1 | -1) => {
    rowRef.current?.scrollBy({ left: direction * rowRef.current.clientWidth * 0.8, behavior: 'smooth' })
  }

  if (titles.length === 0) return null

  return (
    <section ref={sectionRef} className="mx-auto mt-12 w-full max-w-7xl px-4 sm:px-6">
      <div className="mb-4 flex items-end justify-between">
        <h2 className="font-display text-xl font-medium tracking-tight text-paper sm:text-2xl">{heading}</h2>
        <div className="hidden gap-1 sm:flex">
          <button
            onClick={() => scrollBy(-1)}
            aria-label={`Scroll ${heading} left`}
            className="rounded-full border border-line p-1.5 text-muted transition-colors hover:border-amber/50 hover:text-amber"
          >
            <ChevronLeft className="h-4 w-4" />
          </button>
          <button
            onClick={() => scrollBy(1)}
            aria-label={`Scroll ${heading} right`}
            className="rounded-full border border-line p-1.5 text-muted transition-colors hover:border-amber/50 hover:text-amber"
          >
            <ChevronRight className="h-4 w-4" />
          </button>
        </div>
      </div>
      <div ref={rowRef} className="scroll-row flex gap-4 overflow-x-auto pb-2">
        {titles.map((title) => (
          <PosterCard key={title.id} title={title} />
        ))}
      </div>
    </section>
  )
}
