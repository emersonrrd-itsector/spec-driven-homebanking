/**
 * TypeScript interfaces and types for API DTOs
 * Ensures strict typing for all API request/response objects
 */

// Auth DTOs
export interface LoginRequest {
  email: string
  password: string
}

export interface UserDto {
  id: string
  email: string
}

export interface LoginResponse {
  token: string
  expiresIn: number
  user: UserDto
}

// Account DTOs
export interface AccountDto {
  id: string
  name: string
  balance: number
  currency: string
  lastUpdated: string
}

export interface AccountsResponse {
  accounts: AccountDto[]
}

// Transaction DTOs
export type TransactionType = 'Debit' | 'Credit'
export type TransactionCategory =
  | 'Groceries'
  | 'Utilities'
  | 'Entertainment'
  | 'Food & Dining'
  | 'Transport'
  | 'Salary'
  | 'Transfer'
  | 'Other'

export interface TransactionDto {
  id: string
  accountId: string
  amount: number
  type: TransactionType
  date: string
  description: string
  category: TransactionCategory
}

export interface TransactionsResponse {
  transactions: TransactionDto[]
  total: number
  skip: number
  take: number
}

export interface TransactionFilter {
  accountId: string
  skip?: number
  take?: number
  category?: TransactionCategory
}

// Transfer DTOs
export interface TransferRequest {
  fromAccountId: string
  toAccountId: string
  amount: number
  description?: string
}

export interface TransferAccountResponse {
  id: string
  newBalance: number
}

export interface TransferResponse {
  transferId: string
  status: string
  fromAccount: TransferAccountResponse
  toAccount: TransferAccountResponse
  amount: number
}

// Error Response
export interface ErrorResponse {
  code: string
  message: string
  details?: Record<string, unknown>
  timestamp: string
}

// API Error wrapper
export interface ApiError extends Error {
  response?: {
    status: number
    data: ErrorResponse
  }
}
