import axios, { AxiosError } from 'axios'
import type { ProblemDetails } from './types'

export const TOKEN_KEY = 'marquee.token'
export const USER_KEY = 'marquee.user'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL as string,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

/** Extracts a human-readable message from an API error (RFC 7807 ProblemDetails or network failure). */
export function errorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const problem = error.response?.data as ProblemDetails | undefined
    if (problem?.detail) return problem.detail
    if (error.response?.status === 401) return 'You need to sign in to do that.'
    if (!error.response) return 'Cannot reach the server. Is the API running?'
  }
  return 'Something went wrong. Please try again.'
}
