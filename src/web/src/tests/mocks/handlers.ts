/**
 * MSW request handlers for API mocking
 * These handlers intercept API calls during testing
 */

import { http, HttpResponse } from 'msw'
import type {
  LoginResponse,
  AccountDto,
  AccountsResponse,
  TransactionsResponse,
  TransactionDto,
  TransferResponse,
  ErrorResponse,
} from '../../types'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000'

// Mock data
const mockUser = {
  id: '550e8400-e29b-41d4-a716-446655440000',
  email: 'admin@homebank.local',
}

const mockAccounts: AccountDto[] = [
  {
    id: 'acc-001',
    name: 'Checking',
    balance: 5000,
    currency: 'USD',
    lastUpdated: '2026-02-17T12:00:00Z',
  },
  {
    id: 'acc-002',
    name: 'Savings',
    balance: 10000,
    currency: 'USD',
    lastUpdated: '2026-02-17T12:00:00Z',
  },
]

const mockTransactions: TransactionDto[] = [
  {
    id: 'txn-001',
    accountId: 'acc-001',
    amount: 50,
    type: 'Debit',
    date: '2026-02-16T10:30:00Z',
    description: 'Coffee Shop',
    category: 'Food & Dining',
  },
  {
    id: 'txn-002',
    accountId: 'acc-001',
    amount: 120,
    type: 'Debit',
    date: '2026-02-15T08:00:00Z',
    description: 'Electric Bill',
    category: 'Utilities',
  },
  {
    id: 'txn-003',
    accountId: 'acc-001',
    amount: 2000,
    type: 'Credit',
    date: '2026-02-14T09:00:00Z',
    description: 'Salary Deposit',
    category: 'Salary',
  },
]

/**
 * Success handlers: Return valid responses
 */
export const successHandlers = [
  http.post(`${API_BASE_URL}/api/auth`, async ({ request }) => {
    const body = (await request.json()) as { email: string; password: string }

    if (body.email === 'admin@homebank.local' && body.password === 'demo123') {
      const response: LoginResponse = {
        token:
          'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJlbWFpbCI6ImFkbWluQGhvbWViYW5rLmxvY2FsIiwiaWF0IjoxNjQ2MDczMjAwLCJleHAiOjE2NDYxNTk2MDB9.vHkWGEV0yGnDBhL0',
        expiresIn: 86400,
        user: mockUser,
      }
      return HttpResponse.json(response, { status: 200 })
    }

    const error: ErrorResponse = {
      code: 'InvalidCredentials',
      message: 'Email or password incorrect',
      details: undefined,
      timestamp: new Date().toISOString(),
    }
    return HttpResponse.json(error, { status: 401 })
  }),

  http.get(`${API_BASE_URL}/api/accounts`, () => {
    const response: AccountsResponse = {
      accounts: mockAccounts,
    }
    return HttpResponse.json(response, { status: 200 })
  }),

  http.get(`${API_BASE_URL}/api/accounts/:id`, ({ params }) => {
    const account = mockAccounts.find((a) => a.id === params.id)
    if (account) {
      return HttpResponse.json(account, { status: 200 })
    }

    const error: ErrorResponse = {
      code: 'NotFound',
      message: `Account with id ${params.id} not found`,
      details: undefined,
      timestamp: new Date().toISOString(),
    }
    return HttpResponse.json(error, { status: 404 })
  }),

  http.get(`${API_BASE_URL}/api/transactions`, ({ request }) => {
    const url = new URL(request.url)
    const accountId = url.searchParams.get('accountId')
    const skip = parseInt(url.searchParams.get('skip') || '0')
    const take = parseInt(url.searchParams.get('take') || '10')
    const category = url.searchParams.get('category')

    let filtered = mockTransactions.filter((t) => t.accountId === accountId)

    if (category) {
      filtered = filtered.filter((t) => t.category === category)
    }

    const total = filtered.length
    const paginated = filtered.slice(skip, skip + take)

    const response: TransactionsResponse = {
      transactions: paginated,
      total,
      skip,
      take,
    }
    return HttpResponse.json(response, { status: 200 })
  }),

  http.post(`${API_BASE_URL}/api/transfers`, async () => {
    const response: TransferResponse = {
      transferId: 'xfr-001',
      status: 'completed',
      fromAccount: {
        id: 'acc-001',
        newBalance: 4500,
      },
      toAccount: {
        id: 'acc-002',
        newBalance: 10500,
      },
      amount: 500,
    }
    return HttpResponse.json(response, { status: 201 })
  }),
]

/**
 * Error handlers: Return error responses
 */
export const errorHandlers = {
  loginError: http.post(`${API_BASE_URL}/api/auth`, () => {
    const error: ErrorResponse = {
      code: 'InvalidCredentials',
      message: 'Email or password incorrect',
      details: undefined,
      timestamp: new Date().toISOString(),
    }
    return HttpResponse.json(error, { status: 401 })
  }),

  accountNotFound: http.get(`${API_BASE_URL}/api/accounts/:id`, () => {
    const error: ErrorResponse = {
      code: 'NotFound',
      message: 'Account not found',
      details: undefined,
      timestamp: new Date().toISOString(),
    }
    return HttpResponse.json(error, { status: 404 })
  }),

  insufficientBalance: http.post(`${API_BASE_URL}/api/transfers`, () => {
    const error: ErrorResponse = {
      code: 'InsufficientBalance',
      message: 'Transfer amount exceeds available balance',
      details: {
        available: 100,
        requested: 500,
      },
      timestamp: new Date().toISOString(),
    }
    return HttpResponse.json(error, { status: 400 })
  }),

  invalidTransfer: http.post(`${API_BASE_URL}/api/transfers`, () => {
    const error: ErrorResponse = {
      code: 'InvalidTransfer',
      message: 'Cannot transfer to the same account',
      details: undefined,
      timestamp: new Date().toISOString(),
    }
    return HttpResponse.json(error, { status: 400 })
  }),

  serverError: http.get(`${API_BASE_URL}/api/accounts`, () => {
    const error: ErrorResponse = {
      code: 'InternalServerError',
      message: 'An unexpected error occurred',
      details: undefined,
      timestamp: new Date().toISOString(),
    }
    return HttpResponse.json(error, { status: 500 })
  }),
}

export { mockUser, mockAccounts, mockTransactions }
