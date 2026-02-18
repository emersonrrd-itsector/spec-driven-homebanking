import api, { AUTH_TOKEN_KEY } from './api'
import type { LoginRequest, LoginResponse, UserDto } from '../types'

/**
 * AuthService: Handles user authentication and JWT token management
 */
class AuthService {
  /**
   * Login with email and password
   * Stores JWT token in localStorage on success
   * @param email User email
   * @param password User password
   * @returns Login response with JWT token and user info
   */
  async login(email: string, password: string): Promise<LoginResponse> {
    const request: LoginRequest = { email, password }
    const response = await api.post<LoginResponse>('/api/auth', request)

    if (response.data.token) {
      localStorage.setItem(AUTH_TOKEN_KEY, response.data.token)
    }

    return response.data
  }

  /**
   * Logout: Clear JWT token from localStorage
   */
  logout(): void {
    localStorage.removeItem(AUTH_TOKEN_KEY)
  }

  /**
   * Get current user from token (if available)
   * @returns User info or null if not logged in
   */
  getCurrentUser(): UserDto | null {
    const token = localStorage.getItem(AUTH_TOKEN_KEY)
    if (!token) {
      return null
    }

    try {
      // Decode JWT (format: header.payload.signature)
      const parts = token.split('.')
      if (parts.length !== 3) {
        return null
      }

      // Decode payload (with proper padding)
      const payload = JSON.parse(
        atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'))
      )

      // Extract user info from JWT claims
      return {
        id: payload.sub || payload.userId || '',
        email: payload.email || '',
      }
    } catch {
      return null
    }
  }

  /**
   * Check if user is authenticated
   * @returns true if valid JWT exists
   */
  isAuthenticated(): boolean {
    return !!localStorage.getItem(AUTH_TOKEN_KEY)
  }

  /**
   * Get stored JWT token
   * @returns JWT token or null
   */
  getToken(): string | null {
    return localStorage.getItem(AUTH_TOKEN_KEY)
  }
}

export default new AuthService()
