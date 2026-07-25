import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@/lib/adapters/auth'
import { getMe, registerUser } from '@/lib/api/auth-api'

// Resolves the local BusStop profile for the authenticated, email-verified user.
// A 404 from GET /auth/me means the local record does not exist yet, so it is
// created via POST /auth/register (SPEC-IdentityAccess-RegisterFlow step 4).
// The query is keyed by the Keycloak sub so profiles never leak across accounts.
export function useMyProfile() {
  const { isAuthenticated, user } = useAuth()
  const sub = user?.id
  const email = user?.email

  return useQuery({
    queryKey: ['me', sub, email],
    queryFn: async () => {
      const me = await getMe()
      if (me) return me
      if (!email) throw new Error('Missing email in token.')
      return registerUser(email)
    },
    enabled: isAuthenticated && !!sub && user?.emailVerified === true,
    staleTime: 5 * 60 * 1000,
  })
}
