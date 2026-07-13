import { useNavigate } from '@tanstack/react-router'
import { useEffect } from 'react'

export function useAuthRedirect(isAuthenticated: boolean, to = '/') {
  const navigate = useNavigate()

  useEffect(() => {
    if (isAuthenticated) {
      navigate({ to })
    }
  }, [isAuthenticated, navigate, to])
}
