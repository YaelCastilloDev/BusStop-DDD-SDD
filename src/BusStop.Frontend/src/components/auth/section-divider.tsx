interface SectionDividerProps {
  label: string
}

export function SectionDivider({ label }: SectionDividerProps) {
  return (
    <div className='flex items-center justify-center gap-2'>
      <hr className='grow border' />
      <p className='text-base text-foreground font-medium'>{label}</p>
      <hr className='grow border' />
    </div>
  )
}
