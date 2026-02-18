import '@testing-library/jest-dom'
import { afterEach, vi, beforeAll, afterAll } from 'vitest'
import { cleanup } from '@testing-library/react'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'
import { webcrypto } from 'crypto'

// Ensure test environment variables are set to match MSW
const API_TEST_URL = 'http://localhost:5000'

// Force VITE_API_URL to match the MSW handlers URL using Vitest stubEnv
vi.stubEnv('VITE_API_URL', API_TEST_URL)

// Polyfill crypto for Node.js
if (!globalThis.crypto) {
  globalThis.crypto = webcrypto as Crypto
}

// Polyfill getRandomValues if needed
if (globalThis.crypto && !globalThis.crypto.getRandomValues) {
  globalThis.crypto.getRandomValues = (arr: any) => {
    for (let i = 0; i < arr.length; i++) {
      arr[i] = Math.floor(Math.random() * 256)
    }
    return arr
  }
}

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
})

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {}

  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => {
      store[key] = value.toString()
    },
    removeItem: (key: string) => {
      delete store[key]
    },
    clear: () => {
      store = {}
    },
  }
})()

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
})

// Mock window.location
Object.defineProperty(window, 'location', {
  value: {
    ...window.location,
    href: 'http://localhost:3000',
    origin: 'http://localhost:3000',
    protocol: 'http:',
    host: 'localhost:3000',
    hostname: 'localhost',
    port: '3000',
    pathname: '/',
    reload: vi.fn(),
  },
  writable: true,
})

// Setup MSW server with default handlers
export const server = setupServer(
  // Default handlers can be overridden per test
  http.post(`${API_TEST_URL}/api/auth`, async () => {
    return HttpResponse.json(
      {
        code: 'InvalidCredentials',
        message: 'Email or password incorrect',
        details: null,
        timestamp: new Date().toISOString(),
      },
      { status: 401 }
    )
  })
)

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))
afterEach(() => {
  server.resetHandlers()
  cleanup()
  localStorageMock.clear()
})
afterAll(() => server.close())
