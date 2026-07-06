import { useTranslation } from 'react-i18next'
import { Menu, Share2, Printer, MapPinPlus, MapPinOff, X } from 'lucide-react'
import {
  Sidebar,
  SidebarContent,
  useSidebar,
} from '@/components/ui/sidebar'
import { useMapUIStore } from '@/stores/map-ui-store'

export function MainSidebar() {
  const { t } = useTranslation('navigation')
  const { setOpen } = useSidebar()
  const interactionMode = useMapUIStore((s) => s.interactionMode)
  const setInteractionMode = useMapUIStore((s) => s.setInteractionMode)

  const isAddingStop = interactionMode === 'add-stop'

  const handleAddMissingPlace = () => {
    setInteractionMode(isAddingStop ? 'browse' : 'add-stop')
  }

  const menuItems: Array<{
    icon: React.ComponentType<{ className?: string }> | null
    label: string
    indent?: boolean
    onClick?: () => void
    active?: boolean
  }> = [
    {
      icon: Share2,
      label: t('shareEmbed'),
    },
    {
      icon: Printer,
      label: t('print'),
    },
    {
      icon: isAddingStop ? MapPinOff : MapPinPlus,
      label: isAddingStop ? t('stopRegistering') : t('addMissingPlace'),
      indent: true,
      onClick: handleAddMissingPlace,
      active: isAddingStop,
    },
  ]

  return (
    <Sidebar collapsible='icon' className='z-70 border-r'>
      <SidebarContent className='p-0 gap-0'>
        <button
          onClick={() => setOpen(true)}
          className='group-data-[collapsible=icon]:flex hidden items-center justify-center w-full py-3 text-muted-foreground hover:bg-sidebar-accent hover:rounded-r-[20px]'
        >
          <Menu className='size-6' />
        </button>

        <div className='group-data-[collapsible=icon]:hidden flex items-center justify-between px-6 py-3'>
          <h2 className='text-h4 truncate'>BusStop</h2>
          <button
            onClick={() => setOpen(false)}
            className='rounded-md p-1 hover:bg-sidebar-accent text-muted-foreground'
          >
            <X className='size-5' />
          </button>
        </div>

        <ul className='w-full'>
          {menuItems.map((item) => (
            <li
              key={item.label}
              className={!item.icon ? 'group-data-[collapsible=icon]:hidden' : ''}
            >
              <button
                className='flex w-full items-center px-6 py-3 text-start transition-all duration-150 bg-transparent border-none cursor-pointer text-sidebar-foreground no-underline hover:bg-sidebar-accent hover:rounded-r-[20px] group-data-[collapsible=icon]:justify-center group-data-[collapsible=icon]:px-0 group-data-[collapsible=icon]:mr-0 mr-2'
                style={item.indent ? { paddingLeft: '64px' } : undefined}
                onClick={item.onClick}
              >
                {item.icon && (
                  <item.icon
                    className={
                      item.active
                        ? 'size-6 shrink-0 text-primary group-data-[collapsible=icon]:mr-0 mr-4'
                        : 'size-6 shrink-0 text-muted-foreground group-data-[collapsible=icon]:mr-0 mr-4'
                    }
                  />
                )}
                <span
                  className={`text-body-sm tracking-[0.25px] overflow-hidden whitespace-nowrap group-data-[collapsible=icon]:hidden ${
                    item.active ? 'text-primary' : ''
                  }`}
                >
                  {item.label}
                </span>
              </button>
            </li>
          ))}
        </ul>
      </SidebarContent>
    </Sidebar>
  )
}
