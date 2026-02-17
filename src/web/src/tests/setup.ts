import '@testing-library/jest-dom'
import { afterEach, vi, beforeAll, afterAll } from 'vitest'
import { cleanup } from '@testing-library/react'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'
import { webcrypto } from 'crypto'

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

// Mock window.location.href
delete (window.location as any).href
Object.defineProperty(window.location, 'href', {
  writable: true,
  value: '',
})

// Setup MSW server with default handlers
export const server = setupServer(
  // Default handlers can be overridden per test
  http.post(`${import.meta.env.VITE_API_URL || 'http://localhost:5000'}/api/auth`, async () => {
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
