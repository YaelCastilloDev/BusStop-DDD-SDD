import axios from 'axios'
import { parseApiError } from './auth-api'
import type { Country } from './types'

export async function listCountries(): Promise<Country[]> {
  try {
    const response = await axios.get<Country[]>('/countries')
    return response.data
  } catch (error) {
    throw parseApiError(error, 'Failed to load countries. Please try again.')
  }
}
