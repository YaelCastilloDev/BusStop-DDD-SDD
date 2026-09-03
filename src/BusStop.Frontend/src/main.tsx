import { StrictMode } from 'react'
import ReactDOM from 'react-dom/client'
import { KcPage } from './keycloak-theme/kc.gen'
import './styles/index.css'

const rootElement = document.getElementById('root')!
const { kcContext } = window

if (!rootElement.innerHTML) {
  if (kcContext !== undefined) {
    ReactDOM.createRoot(rootElement).render(
      <StrictMode>
        <KcPage kcContext={kcContext} />
      </StrictMode>
    )
  } else {
    // Keycloak must process the authorization callback before TanStack Router
    // reads or mutates the URL. Theme pages never initialize the SPA adapter.
    const { initializeAuth } = await import('@/lib/adapters/auth')
    await initializeAuth()

    const { renderApplication } = await import('./application')
    renderApplication(rootElement)
  }
}
