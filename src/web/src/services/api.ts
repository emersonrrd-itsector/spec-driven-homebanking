import axios, { AxiosError } from 'axios'
import type { AxiosInstance } from 'axios'
import type { ErrorResponse } from '../types'

/**
 * Create and configure the Axios instance for API calls
 * Includes request/response interceptors for authentication and error handling
 */

const AUTH_TOKEN_KEY = 'homebank_jwt'
const DEFAULT_API_URL = 'http://localhost:5087'

// Create axios instance with validated baseURL
function createApiClient(): AxiosInstance {
  const baseURL = import.meta.env.VITE_API_URL || DEFAULT_API_URL
  
  const api: AxiosInstance = axios.create({
    baseURL,
    headers: {
      'Content-Type': 'application/json',
    },
  })

  /**
   * Request interceptor: Add Authorization header with JWT token
   */
  api.interceptors.request.use(
    (config) => {
      const token = localStorage.getItem(AUTH_TOKEN_KEY)
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
      return config
    },
    (error) => {
      return Promise.reject(error)
    }
  )

  /**
   * Response interceptor: Handle errors and redirect on 401
   */
  api.interceptors.response.use(
    (response) => response,
    (error: AxiosError<ErrorResponse>) => {
      if (error.response?.status === 401) {
        // Clear token and redirect to login
        localStorage.removeItem(AUTH_TOKEN_KEY)
        try {
          // Use absolute URL for navigation to avoid URL parsing issues
          window.location.href = window.location.origin + '/login'
        } catch (navError) {
          console.warn('[API] Failed to redirect to login:', navError)
        }
      }
      return Promise.reject(error)
    }
  )

  return api
}

// Create singleton instance
const api = createApiClient()

export default api
export { AUTH_TOKEN_KEY }
