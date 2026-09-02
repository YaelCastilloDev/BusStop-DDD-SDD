import * as React from 'react'
import { type VariantProps } from 'class-variance-authority'
import { motion, useReducedMotion } from 'framer-motion'
import { cn } from '@/lib/utils'
import { buttonVariants } from '@/components/ui/button'

type ShimmerButtonProps = Omit<
  React.ComponentProps<typeof motion.button>,
  'children' | 'className'
> &
  VariantProps<typeof buttonVariants> & {
    className?: string
    children: React.ReactNode
  }

function ShimmerButton({
  className,
  variant,
  size,
  type = 'button',
  children,
  ...props
}: ShimmerButtonProps) {
  const prefersReducedMotion = useReducedMotion()

  return (
    <motion.button
      type={type}
      className={cn(
        buttonVariants({ variant, size }),
        'relative overflow-hidden bg-[linear-gradient(120deg,var(--color-primary)_0%,var(--color-primary-foreground)_50%,var(--color-primary)_100%)] bg-[length:200%_100%] motion-reduce:transition-none',
        className
      )}
      initial={false}
      animate={
        prefersReducedMotion
          ? { backgroundPositionX: '0%' }
          : { backgroundPositionX: ['200%', '-200%'] }
      }
      transition={
        prefersReducedMotion
          ? { duration: 0 }
          : {
              backgroundPositionX: {
                duration: 3,
                ease: 'linear',
                repeat: Infinity,
              },
            }
      }
      whileHover={prefersReducedMotion ? undefined : { scale: 1.02 }}
      whileTap={prefersReducedMotion ? undefined : { scale: 0.98 }}
      {...props}
    >
      <span className='relative text-label'>{children}</span>
    </motion.button>
  )
}

export { ShimmerButton, type ShimmerButtonProps }
