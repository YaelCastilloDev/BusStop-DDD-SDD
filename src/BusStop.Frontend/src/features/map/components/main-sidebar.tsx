import { useTranslation } from 'react-i18next'
import { Link, useLocation } from '@tanstack/react-router'
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarSeparator,
  useSidebar,
} from '@/components/ui/sidebar'
import { MapPin } from 'lucide-react'

const navItems = [
  {
    to: '/',
    icon: MapPin,
    labelKey: 'navigation:mapExplorer',
  },
]

function MainNav() {
  const { t } = useTranslation()
  const location = useLocation()
  const { open } = useSidebar()

  return (
    <SidebarGroup>
      <SidebarGroupContent>
        <SidebarMenu>
          {navItems.map((item) => {
            const isActive = location.pathname === item.to
            const Icon = item.icon

            return (
              <SidebarMenuItem key={item.to}>
                <SidebarMenuButton
                  asChild
                  isActive={isActive}
                  tooltip={t(item.labelKey)}
                  size='default'
                >
                  <Link to={item.to}>
                    <Icon />
                    <span>{t(item.labelKey)}</span>
                  </Link>
                </SidebarMenuButton>
              </SidebarMenuItem>
            )
          })}
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  )
}

export function MainSidebar() {
  const { t } = useTranslation()
  const { open } = useSidebar()

  return (
    <Sidebar collapsible='icon' className='z-50 top-[--topbar-height] h-[calc(100svh-var(--topbar-height))] border-r'>
      <SidebarHeader className='h-14 justify-center border-b'>
        <h2 className='truncate text-sm font-semibold group-data-[collapsible=icon]:hidden'>
          {t('navigation:mapExplorer')}
        </h2>
      </SidebarHeader>

      <SidebarContent>
        <MainNav />
      </SidebarContent>

      <SidebarFooter>
        <SidebarSeparator />
        <p className='px-2 text-xs text-muted-foreground group-data-[collapsible=icon]:hidden'>
          BusStop v1.0
        </p>
      </SidebarFooter>
    </Sidebar>
  )
}
