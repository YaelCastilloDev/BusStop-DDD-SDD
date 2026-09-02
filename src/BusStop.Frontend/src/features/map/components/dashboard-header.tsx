import { useNavigate } from '@tanstack/react-router'
import { LogOut } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/lib/adapters/auth'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Separator } from '@/components/ui/separator'
import { ShimmerButton } from '@/components/ui/shimmer-button'
import { SidebarTrigger } from '@/components/ui/sidebar'

export function DashboardHeader() {
  const { t } = useTranslation('common')
  const { t: tNavigation } = useTranslation('navigation')
  const navigate = useNavigate()
  const { isAuthenticated, user, logout } = useAuth()
  const displayName = user?.firstName || user?.username || t('account')

  return (
    <header className='z-60 flex h-16 shrink-0 items-center gap-2 border-b bg-background px-4 md:px-6'>
      <div className='flex items-center gap-2'>
        <SidebarTrigger aria-label={t('toggleSidebar')} />
        <Separator orientation='vertical' className='h-5' />
        <h1 className='hidden text-label text-foreground sm:block'>
          {tNavigation('mapExplorer')}
        </h1>
      </div>

      <div className='ml-auto flex items-center gap-2'>
        {isAuthenticated && user ? (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant='ghost'
                className='h-auto gap-2 px-2 py-1.5'
                aria-label={t('accountMenu')}
              >
                <Avatar>
                  <AvatarFallback>
                    {displayName.slice(0, 1).toUpperCase()}
                  </AvatarFallback>
                </Avatar>
                <span className='hidden text-label md:inline'>
                  {displayName}
                </span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align='end' className='z-110'>
              <DropdownMenuLabel>
                <span className='block text-label'>{displayName}</span>
                <span className='block text-caption text-muted-foreground'>
                  {user.username}
                </span>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem onSelect={() => logout()}>
                <LogOut className='size-4' />
                {t('logout')}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        ) : (
          <ShimmerButton
            variant='default'
            size='sm'
            onClick={() => navigate({ to: '/login' })}
          >
            {t('signIn')}
          </ShimmerButton>
        )}
      </div>
    </header>
  )
}
