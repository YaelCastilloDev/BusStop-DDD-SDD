import { Button } from '@/components/ui/button'
import { X } from 'lucide-react'

interface DetailPanelHeaderProps {
  title: string
  onClose: () => void
  closeLabel: string
}

export function DetailPanelHeader({ title, onClose, closeLabel }: DetailPanelHeaderProps) {
  return (
    <div className='flex items-center justify-between border-b px-4 py-3'>
      <h2 className='text-sm font-semibold'>{title}</h2>
      <Button
        variant='ghost'
        size='icon'
        className='size-8'
        onClick={onClose}
        aria-label={closeLabel}
      >
        <X className='size-4' />
      </Button>
    </div>
  )
}
