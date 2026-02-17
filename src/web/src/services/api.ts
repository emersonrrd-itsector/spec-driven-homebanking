import axios, { AxiosError } from 'axios'
import type { AxiosInstance } from 'axios'
import type { ErrorResponse } from '../types'

/**
 * Create and configure the Axios instance for API calls
 * Includes request/response interceptors for authentication and error handling
 */

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'
const AUTH_TOKEN_KEY = 'homebank_jwt'

const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
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
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
export { AUTH_TOKEN_KEY, API_BASE_URL }
