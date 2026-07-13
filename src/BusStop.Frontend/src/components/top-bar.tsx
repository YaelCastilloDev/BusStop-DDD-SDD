import { useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from '@tanstack/react-router'
import { Button } from '@/components/ui/button'
import { useSidebar } from '@/components/ui/sidebar'
import { useAuth } from '@/lib/adapters/auth'
import { cn } from '@/lib/utils'
import { Menu, LogOut } from 'lucide-react'

interface TopBarProps {
  children?: ReactNode
}

export function TopBar({ children }: TopBarProps) {
  const { t } = useTranslation('common')
  const { toggleSidebar } = useSidebar()
  const navigate = useNavigate()
  const { isAuthenticated, user, logout } = useAuth()
  const [signInHovered, setSignInHovered] = useState(false)

  return (
    <header className='sticky top-0 z-60 flex h-16 shrink-0 items-center border-b border-border bg-background px-4 shadow-sm md:px-6'>
      <div className='flex items-center gap-2'>
        <Button
          variant='ghost'
          size='icon'
          onClick={toggleSidebar}
          aria-label={t('toggleSidebar')}
        >
          <Menu className='size-5' />
        </Button>

        <nav className='hidden items-center gap-1 md:flex'>
          {children}
        </nav>
      </div>

      <div className='ml-auto flex items-center gap-4 md:gap-6'>
        {isAuthenticated && user ? (
          <div className='flex items-center gap-2'>
            <span className='hidden text-label text-muted-foreground md:inline'>
              {user.firstName || user.username}
            </span>
            <Button variant='ghost' size='icon' aria-label={t('logout')} onClick={() => logout()}>
              <LogOut className='size-5' />
            </Button>
          </div>
        ) : (
          <Button
            variant='header'
            size='sm'
            className={cn('rounded-none', signInHovered && 'text-foreground border-destructive')}
            onMouseEnter={() => setSignInHovered(true)}
            onMouseLeave={() => setSignInHovered(false)}
            onClick={() => navigate({ to: '/login' })}
          >
            {t('signIn')}
          </Button>
        )}
      </div>
    </header>
  )
}
