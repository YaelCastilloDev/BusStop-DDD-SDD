import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { ThemeSwitch } from '@/components/theme-switch'
import { SidebarTrigger } from '@/components/ui/sidebar'
import { Bus } from 'lucide-react'

export function TopBar() {
  const { t } = useTranslation('common')

  return (
    <header className='sticky top-0 z-60 flex h-14 shrink-0 items-center border-b bg-background/80 px-4 backdrop-blur-lg'>
      <div className='ml-auto flex items-center gap-3'>
        <SidebarTrigger />
        <span className='hidden text-sm font-semibold sm:inline-block'>
          BusStop
        </span>
        <Button
          variant='outline'
          size='sm'
          onClick={() => {
            alert(t('comingSoon'))
          }}
        >
          {t('register')}
        </Button>
        <Bus className='size-5 text-primary' />
        <ThemeSwitch />
      </div>
    </header>
  )
}
