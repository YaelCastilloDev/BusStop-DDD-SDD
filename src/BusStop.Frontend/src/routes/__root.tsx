import { type QueryClient } from '@tanstack/react-query'
import { createRootRouteWithContext, Outlet } from '@tanstack/react-router'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools'
import { Toaster } from '@/components/ui/sonner'
import { Button } from '@/components/ui/button'
import { Link } from '@tanstack/react-router'

function NotFound() {
  return (
    <div className='flex h-svh flex-col items-center justify-center gap-4'>
      <h1 className='text-4xl font-bold'>404</h1>
      <p className='text-muted-foreground'>Page not found</p>
      <Button asChild variant='outline'>
        <Link to='/'>Go Home</Link>
      </Button>
    </div>
  )
}

function ErrorFallback() {
  return (
    <div className='flex h-svh flex-col items-center justify-center gap-4'>
      <h1 className='text-4xl font-bold'>Error</h1>
      <p className='text-muted-foreground'>Something went wrong</p>
      <Button asChild variant='outline'>
        <Link to='/'>Go Home</Link>
      </Button>
    </div>
  )
}

export const Route = createRootRouteWithContext<{
  queryClient: QueryClient
}>()({
  component: () => {
    return (
      <>
        <Outlet />
        <Toaster duration={5000} />
        {import.meta.env.MODE === 'development' && (
          <>
            <ReactQueryDevtools buttonPosition='bottom-left' />
            <TanStackRouterDevtools position='bottom-right' />
          </>
        )}
      </>
    )
  },
  notFoundComponent: NotFound,
  errorComponent: ErrorFallback,
})
