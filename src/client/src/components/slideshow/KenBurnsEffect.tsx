import { useEffect, useState } from 'react'
import { AuthenticatedImage } from '../photos/AuthenticatedImage'

interface KenBurnsEffectProps {
  src: string
  alt: string
  direction?: 'zoom-in' | 'zoom-out' | 'pan-left' | 'pan-right' | 'random'
  className?: string
}

const DIRECTIONS = ['zoom-in', 'zoom-out', 'pan-left', 'pan-right'] as const

export function KenBurnsEffect({ src, alt, direction = 'random', className = '' }: KenBurnsEffectProps) {
  const [currentDirection, setCurrentDirection] = useState<typeof DIRECTIONS[number]>(() => {
    if (direction === 'random') {
      return DIRECTIONS[Math.floor(Math.random() * DIRECTIONS.length)]
    }
    return direction
  })

  const [key, setKey] = useState(0)

  // Reset animation when src changes
  useEffect(() => {
    setKey((prev) => prev + 1)
    if (direction === 'random') {
      setCurrentDirection(DIRECTIONS[Math.floor(Math.random() * DIRECTIONS.length)])
    }
  }, [src, direction])

  const getTransformStyle = () => {
    const baseStyle = 'transition-transform duration-[5000ms] ease-in-out'

    switch (currentDirection) {
      case 'zoom-in':
        return `${baseStyle} scale-100 hover:scale-110`
      case 'zoom-out':
        return `${baseStyle} scale-110 hover:scale-100`
      case 'pan-left':
        return `${baseStyle} translate-x-4 hover:translate-x-0`
      case 'pan-right':
        return `${baseStyle} -translate-x-4 hover:translate-x-0`
      default:
        return baseStyle
    }
  }

  return (
    <div className="relative overflow-hidden">
      <AuthenticatedImage
        key={key}
        src={src}
        alt={alt}
        className={`${className} ${getTransformStyle()}`}
      />
    </div>
  )
}
