// Mirrors BusStop.UseCases contracts (camelCase JSON).
// UserResponse: BusStop.UseCases/Users/UserResponse.cs
export interface BusStopUser {
  id: number
  username: string | null
  email: string
  externalId: string | null
  createdAt: string
  countryId: number | null
}

// CountryResponse: BusStop.UseCases/Countries/CountryResponse.cs
export interface Country {
  id: number
  name: string
  isoCode: string
}
