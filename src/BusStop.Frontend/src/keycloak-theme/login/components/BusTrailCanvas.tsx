import { useEffect, useRef } from 'react'
import { motion, useMotionValue, useSpring, useTransform } from 'framer-motion'

const BUS_COLOR = 'hsl(var(--primary))'

export function BusTrailCanvas() {
  const mouseX = useMotionValue(window.innerWidth / 2)
  const mouseY = useMotionValue(window.innerHeight / 2)

  const springX = useSpring(mouseX, { stiffness: 60, damping: 24, mass: 0.8 })
  const springY = useSpring(mouseY, { stiffness: 60, damping: 24, mass: 0.8 })

  const busRotation = useTransform(
    [springX, springY],
    ([latestX, latestY]: number[]) => {
      const rawX = mouseX.get()
      const rawY = mouseY.get()
      const dx = rawX - latestX
      const dy = rawY - latestY
      return Math.atan2(dy, dx) * (180 / Math.PI) + 90
    }
  )

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      mouseX.set(e.clientX)
      mouseY.set(e.clientY)
    }

    window.addEventListener('mousemove', handleMouseMove, { passive: true })
    return () => window.removeEventListener('mousemove', handleMouseMove)
  }, [mouseX, mouseY])

  return (
    <div className='pointer-events-none absolute inset-0 overflow-hidden'>
      <svg className='absolute inset-0 h-full w-full opacity-[0.04]'>
        <defs>
          <pattern id='grid' width='40' height='40' patternUnits='userSpaceOnUse'>
            <path
              d='M 40 0 L 0 0 0 40'
              fill='none'
              stroke='currentColor'
              strokeWidth='0.5'
            />
          </pattern>
        </defs>
        <rect width='100%' height='100%' fill='url(#grid)' />
      </svg>

      <motion.div
        className='absolute'
        style={{
          x: springX,
          y: springY,
          translateX: '-50%',
          translateY: '-50%',
          rotate: busRotation,
        }}
      >
        <BusIcon />
      </motion.div>

      {[1, 2, 3, 4, 5].map((i) => (
        <TrailDot key={i} springX={springX} springY={springY} index={i} />
      ))}
    </div>
  )
}

function TrailDot({
  springX,
  springY,
  index,
}: {
  springX: ReturnType<typeof useSpring<number>>
  springY: ReturnType<typeof useSpring<number>>
  index: number
}) {
  const delayMs = index * 50

  const delayedX = useSpring(springX, {
    stiffness: 40,
    damping: 20,
    mass: 0.4 + index * 0.1,
    restDelta: 0.5,
  })
  const delayedY = useSpring(springY, {
    stiffness: 40,
    damping: 20,
    mass: 0.4 + index * 0.1,
    restDelta: 0.5,
  })

  useEffect(() => {
    const unsubX = springX.on('change', (v) => {
      setTimeout(() => delayedX.set(v), delayMs)
    })
    const unsubY = springY.on('change', (v) => {
      setTimeout(() => delayedY.set(v), delayMs)
    })
    return () => {
      unsubX()
      unsubY()
    }
  }, [springX, springY, delayedX, delayedY, delayMs])

  return (
    <motion.div
      className='absolute size-2 rounded-full bg-primary/20'
      style={{
        x: delayedX,
        y: delayedY,
        translateX: '-50%',
        translateY: '-50%',
      }}
    />
  )
}

function BusIcon() {
  return (
    <svg
      width='48'
      height='48'
      viewBox='0 0 24 24'
      fill='none'
      stroke={BUS_COLOR}
      strokeWidth='1.5'
      strokeLinecap='round'
      strokeLinejoin='round'
      className='drop-shadow-lg'
    >
      <path d='M8 6v6' />
      <path d='M16 6v6' />
      <rect width='20' height='14' x='2' y='5' rx='2' />
      <circle cx='8' cy='19' r='2' />
      <circle cx='16' cy='19' r='2' />
      <path d='M2 13h20' />
      <path d='M6 5V3h12v2' />
    </svg>
  )
}
