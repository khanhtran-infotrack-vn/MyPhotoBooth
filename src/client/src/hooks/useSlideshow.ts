import { useEffect, useRef, useCallback } from 'react'
import { useSlideshowStore } from '../stores/slideshowStore'
import type { Photo } from '../types'

interface UseSlideshowOptions {
  photos: Photo[]
  onSlideChange?: (index: number) => void
  onEnd?: () => void
}

export function useSlideshow({ photos, onSlideChange, onEnd }: UseSlideshowOptions) {
  const {
    config,
    isPlaying,
    currentIndex,
    setIsPlaying,
    setCurrentIndex
  } = useSlideshowStore()

  const intervalRef = useRef<number | null>(null)
  const shuffledIndicesRef = useRef<number[]>([])
  const currentIndexRef = useRef(currentIndex)

  // Keep ref in sync with state
  useEffect(() => {
    currentIndexRef.current = currentIndex
  }, [currentIndex])

  // Generate shuffled indices if shuffle is enabled
  const generateShuffledIndices = useCallback(() => {
    const indices = photos.map((_, i) => i)
    for (let i = indices.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1))
      ;[indices[i], indices[j]] = [indices[j], indices[i]]
    }
    return indices
  }, [photos])

  // Initialize shuffled indices when photos change or shuffle is toggled
  useEffect(() => {
    if (config.shuffle && photos.length > 0) {
      shuffledIndicesRef.current = generateShuffledIndices()
    }
  }, [config.shuffle, photos, generateShuffledIndices])

  // Get actual index (handles shuffle)
  const getActualIndex = useCallback((index: number) => {
    if (config.shuffle && shuffledIndicesRef.current.length > 0) {
      return shuffledIndicesRef.current[index % shuffledIndicesRef.current.length]
    }
    return index
  }, [config.shuffle])

  // Start/stop slideshow
  useEffect(() => {
    if (!isPlaying || photos.length === 0) {
      if (intervalRef.current) {
        clearInterval(intervalRef.current)
        intervalRef.current = null
      }
      return
    }

    intervalRef.current = setInterval(() => {
      const nextIndex = currentIndexRef.current + 1

      // Check if we've reached the end
      if (nextIndex >= photos.length) {
        if (config.loop) {
          setCurrentIndex(0)
        } else {
          setIsPlaying(false)
          onEnd?.()
        }
      } else {
        setCurrentIndex(nextIndex)
      }
    }, config.timing * 1000)

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current)
      }
    }
  }, [isPlaying, currentIndex, photos.length, config.timing, config.loop, setIsPlaying, onEnd, setCurrentIndex])

  // Handle slide change
  useEffect(() => {
    if (photos.length > 0) {
      const actualIndex = getActualIndex(currentIndex)
      onSlideChange?.(actualIndex)
    }
  }, [currentIndex, photos.length, getActualIndex, onSlideChange])

  // Get current photo
  const getCurrentPhoto = useCallback((): Photo | null => {
    if (photos.length === 0) return null
    const actualIndex = getActualIndex(currentIndex)
    return photos[actualIndex] || null
  }, [photos, currentIndex, getActualIndex])

  // Navigation
  const goToSlide = useCallback((index: number) => {
    if (index >= 0 && index < photos.length) {
      setCurrentIndex(index)
    }
  }, [photos.length, setCurrentIndex])

  const goNext = useCallback(() => {
    const nextIndex = currentIndex + 1
    if (nextIndex >= photos.length) {
      if (config.loop) {
        setCurrentIndex(0)
      } else {
        setIsPlaying(false)
        onEnd?.()
      }
    } else {
      setCurrentIndex(nextIndex)
    }
  }, [currentIndex, photos.length, config.loop, setIsPlaying, onEnd, setCurrentIndex])

  const goPrev = useCallback(() => {
    const prevIndex = currentIndex - 1
    if (prevIndex < 0) {
      if (config.loop) {
        setCurrentIndex(photos.length - 1)
      }
    } else {
      setCurrentIndex(prevIndex)
    }
  }, [currentIndex, photos.length, config.loop, setCurrentIndex])

  // Toggle play/pause
  const togglePlayPause = useCallback(() => {
    setIsPlaying(!isPlaying)
  }, [isPlaying, setIsPlaying])

  // Reset slideshow when photos change significantly
  useEffect(() => {
    setCurrentIndex(0)
    if (config.shuffle) {
      shuffledIndicesRef.current = generateShuffledIndices()
    }
  }, [photos.length, config.shuffle, generateShuffledIndices, setCurrentIndex])

  return {
    isPlaying,
    currentIndex,
    currentPhoto: getCurrentPhoto(),
    totalPhotos: photos.length,
    config,
    nextSlide: goNext,
    prevSlide: goPrev,
    goToSlide,
    togglePlayPause,
    setIsPlaying,
    setCurrentIndex
  }
}
