import { StrictMode } from 'react'
import ReactDOM from 'react-dom/client'
import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios'
import {
  QueryCache,
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query'
import { RouterProvider, createRouter } from '@tanstack/react-router'
import { I18nextProvider } from 'react-i18next'
import { toast } from 'sonner'
import i18n from '@/lib/i18n'
import { getAuthToken } from '@/lib/adapters/auth'
import { createLogger } from '@/lib/logger'
import { DirectionProvider } from './context/direction-provider'
import { FontProvider } from './context/font-provider'
// Generated Routes
import { routeTree } from './routeTree.gen'
// Styles
import './styles/index.css'

const mainLogger = createLogger('main')

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: (_failureCount, error) => {
        if (import.meta.env.DEV) return false

        return !(
          error instanceof AxiosError &&
          [401, 403].includes(error.response?.status ?? 0)
        )
      },
      refetchOnWindowFocus: import.meta.env.PROD,
      staleTime: 10 * 1000,
    },
    mutations: {
      onError: (error) => {
        if (error instanceof AxiosError) {
          if (error.response?.status === 304) {
            mainLogger.warn('mutation returned 304 Not Modified', { url: error.config?.url })
            toast.error('Content not modified!')
          }
        }
      },
    },
  },
  queryCache: new QueryCache({
    onError: (error) => {
      if (error instanceof AxiosError) {
        if (error.response?.status === 401) {
          mainLogger.warn('query returned 401 Unauthorized', { url: error.config?.url })
          toast.error('Session expired!')
        }
        if (error.response?.status === 500) {
          mainLogger.error('query returned 500 Internal Server Error', { url: error.config?.url })
          toast.error('Internal Server Error!')
        }
      }
    },
  }),
})

axios.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    const token = await getAuthToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
      mainLogger.debug('Bearer token attached to request', {
        url: config.url,
        method: config.method,
      })
    } else {
      mainLogger.debug('no token available for request', {
        url: config.url,
        method: config.method,
      })
    }
    return config
  },
  (error) => {
    mainLogger.error('axios request interceptor failed', error)
    return Promise.reject(error)
  }
)

// Create a new router instance
const router = createRouter({
  routeTree,
  context: { queryClient },
  defaultPreload: 'intent',
  defaultPreloadStaleTime: 0,
})

// Register the router instance for type safety
declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}

// Render the app
const rootElement = document.getElementById('root')!
if (!rootElement.innerHTML) {
  const root = ReactDOM.createRoot(rootElement)
  root.render(
    <StrictMode>
      <QueryClientProvider client={queryClient}>
        <I18nextProvider i18n={i18n}>
          <FontProvider>
            <DirectionProvider>
              <RouterProvider router={router} />
            </DirectionProvider>
          </FontProvider>
        </I18nextProvider>
      </QueryClientProvider>
    </StrictMode>
  )
}
