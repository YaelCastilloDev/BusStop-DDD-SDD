import { useQuery } from '@tanstack/react-query'
import { listCountries } from '@/lib/api/countries-api'

export function useCountries(enabled: boolean) {
  return useQuery({
    queryKey: ['countries'],
    queryFn: listCountries,
    enabled,
    staleTime: Infinity,
  })
}
