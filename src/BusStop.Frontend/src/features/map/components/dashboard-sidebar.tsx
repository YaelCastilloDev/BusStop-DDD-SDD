import { Link } from '@tanstack/react-router'
import { MapPin, MapPinOff, MapPinPlus, Printer, Share2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useMapUIStore } from '@/stores/map-ui-store'
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarSeparator,
} from '@/components/ui/sidebar'

export function DashboardSidebar({
  ...props
}: React.ComponentProps<typeof Sidebar>) {
  const { t } = useTranslation('navigation')
  const interactionMode = useMapUIStore((state) => state.interactionMode)
  const setInteractionMode = useMapUIStore((state) => state.setInteractionMode)
  const isAddingStop = interactionMode === 'add-stop'

  const toggleAddStop = () => {
    setInteractionMode(isAddingStop ? 'browse' : 'add-stop')
  }

  return (
    <Sidebar collapsible='icon' className='top-16! z-70 h-auto!' {...props}>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size='lg' tooltip={t('mapExplorer')} asChild>
              <Link to='/' aria-label={t('mapExplorer')}>
                <div className='flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground'>
                  <MapPin className='size-4' />
                </div>
                <span className='text-label'>{t('brand')}</span>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel className='text-label'>
            {t('platform')}
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              <SidebarMenuItem>
                <SidebarMenuButton
                  tooltip={
                    isAddingStop ? t('cancelAddStop') : t('addMissingPlace')
                  }
                  isActive={isAddingStop}
                  onClick={toggleAddStop}
                >
                  {isAddingStop ? <MapPinOff /> : <MapPinPlus />}
                  <span className='text-label'>
                    {isAddingStop ? t('cancelAddStop') : t('addMissingPlace')}
                  </span>
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        <SidebarGroup>
          <SidebarGroupLabel className='text-label'>
            {t('sections')}
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              <SidebarMenuItem>
                <SidebarMenuButton tooltip={t('shareEmbed')} disabled>
                  <Share2 />
                  <span className='text-label'>{t('shareEmbed')}</span>
                </SidebarMenuButton>
              </SidebarMenuItem>
              <SidebarMenuItem>
                <SidebarMenuButton tooltip={t('print')} disabled>
                  <Printer />
                  <span className='text-label'>{t('print')}</span>
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        <SidebarSeparator />
      </SidebarFooter>
    </Sidebar>
  )
}
