import { useState } from 'react'

interface RatingBarProps {
  value: number | null
  onChange: (score: number) => void
  disabled?: boolean
}

/** Ten-segment rating control — tap or hover a segment to score 1-10. */
export default function RatingBar({ value, onChange, disabled }: RatingBarProps) {
  const [hovered, setHovered] = useState<number | null>(null)
  const active = hovered ?? value ?? 0

  return (
    <div
      role="radiogroup"
      aria-label="Your rating from 1 to 10"
      className="flex items-center gap-1"
      onMouseLeave={() => setHovered(null)}
    >
      {Array.from({ length: 10 }, (_, index) => {
        const score = index + 1
        const lit = score <= active
        return (
          <button
            key={score}
            role="radio"
            aria-checked={value === score}
            aria-label={`${score} out of 10`}
            disabled={disabled}
            onMouseEnter={() => setHovered(score)}
            onFocus={() => setHovered(score)}
            onClick={() => onChange(score)}
            className={`h-7 w-5 rounded-xs border transition-all duration-150 sm:w-6 ${
              lit ? 'border-amber bg-amber/85' : 'border-line bg-panel hover:border-amber/40'
            } ${disabled ? 'cursor-not-allowed opacity-50' : 'cursor-pointer'}`}
          />
        )
      })}
      <span className="ml-2 min-w-10 font-display text-lg text-amber">
        {active > 0 ? `${active}/10` : '—'}
      </span>
    </div>
  )
}
