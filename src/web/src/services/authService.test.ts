import { describe, it, expect, afterEach } from 'vitest'
import { server } from '../tests/setup'
import authService from './authService'
import { successHandlers, errorHandlers } from '../tests/mocks/handlers'
import { AUTH_TOKEN_KEY } from './api'

describe('AuthService', () => {
  const validEmail = 'admin@homebank.local'
  const validPassword = 'demo123'

  afterEach(() => {
    localStorage.clear()
  })

  describe('login', () => {
    it('should login with valid credentials and store token', async () => {
      server.use(...successHandlers)

      const result = await authService.login(validEmail, validPassword)

      expect(result.token).toBeDefined()
      expect(result.expiresIn).toBe(86400)
      expect(result.user.email).toBe(validEmail)
      expect(localStorage.getItem(AUTH_TOKEN_KEY)).toBe(result.token)
    })

    it('should throw error with invalid credentials', async () => {
      server.use(errorHandlers.loginError)

      try {
        await authService.login('wrong@email.com', 'wrongpassword')
        expect.fail('Should have thrown error')
      } catch (error: any) {
        expect(error.response?.status).toBe(401)
        expect(error.response?.data.code).toBe('InvalidCredentials')
      }
    })

    it('should throw error with missing email', async () => {
      server.use(errorHandlers.loginError)

      try {
        await authService.login('', validPassword)
        expect.fail('Should have thrown error')
      } catch (error: any) {
        expect(error.response?.status).toBe(401)
      }
    })

    it('should throw error with missing password', async () => {
      server.use(errorHandlers.loginError)

      try {
        await authService.login(validEmail, '')
        expect.fail('Should have thrown error')
      } catch (error: any) {
        expect(error.response?.status).toBe(401)
      }
    })
  })

  describe('logout', () => {
    it('should remove token from localStorage', async () => {
      localStorage.setItem(AUTH_TOKEN_KEY, 'test-token')
      authService.logout()
      expect(localStorage.getItem(AUTH_TOKEN_KEY)).toBeNull()
    })
  })

  describe('isAuthenticated', () => {
    it('should return true when token exists', () => {
      localStorage.setItem(AUTH_TOKEN_KEY, 'test-token')
      expect(authService.isAuthenticated()).toBe(true)
    })

    it('should return false when token does not exist', () => {
      expect(authService.isAuthenticated()).toBe(false)
    })
  })

  describe('getToken', () => {
    it('should return token from localStorage', () => {
      localStorage.setItem(AUTH_TOKEN_KEY, 'test-token')
      expect(authService.getToken()).toBe('test-token')
    })

    it('should return null when no token exists', () => {
      expect(authService.getToken()).toBeNull()
    })
  })

  describe('getCurrentUser', () => {
    it('should return null when no token exists', () => {
      expect(authService.getCurrentUser()).toBeNull()
    })

    it('should extract user info from valid JWT token', () => {
      // Valid JWT with email and sub claims
      const token =
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJlbWFpbCI6ImFkbWluQGhvbWViYW5rLmxvY2FsIn0.test'
      localStorage.setItem(AUTH_TOKEN_KEY, token)

      const user = authService.getCurrentUser()
      expect(user).toBeDefined()
      expect(user?.id).toBe('550e8400-e29b-41d4-a716-446655440000')
      expect(user?.email).toBe('admin@homebank.local')
    })

    it('should return null for invalid JWT format', () => {
      localStorage.setItem(AUTH_TOKEN_KEY, 'invalid-token')
      expect(authService.getCurrentUser()).toBeNull()
    })

    it('should return null for corrupted JWT payload', () => {
      // JWT with invalid base64 payload
      const token = 'eyJhbGciOiJIUzI1NiJ9.invalid!!!.test'
      localStorage.setItem(AUTH_TOKEN_KEY, token)
      expect(authService.getCurrentUser()).toBeNull()
    })
  })
})
