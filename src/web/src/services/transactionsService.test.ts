import { describe, it, expect } from 'vitest'
import { server } from '../tests/setup'
import transactionsService from './transactionsService'
import { successHandlers } from '../tests/mocks/handlers'

describe('TransactionsService', () => {
  describe('getTransactions', () => {
    it('should fetch transactions for account', async () => {
      server.use(...successHandlers)

      const result = await transactionsService.getTransactions('acc-001')

      expect(result).toBeDefined()
      expect(result.transactions).toBeDefined()
      expect(result.transactions.length).toBeGreaterThan(0)
      expect(result.total).toBeDefined()
      expect(result.skip).toBe(0)
      expect(result.take).toBe(10)
    })

    it('should return transactions with all required fields', async () => {
      server.use(...successHandlers)

      const result = await transactionsService.getTransactions('acc-001')
      const transaction = result.transactions[0]

      expect(transaction.id).toBeDefined()
      expect(transaction.accountId).toBe('acc-001')
      expect(transaction.amount).toBeDefined()
      expect(transaction.type).toBeDefined()
      expect(transaction.date).toBeDefined()
      expect(transaction.description).toBeDefined()
      expect(transaction.category).toBeDefined()
    })

    it('should support pagination with skip and take', async () => {
      server.use(...successHandlers)

      const result = await transactionsService.getTransactions('acc-001', 0, 5)

      expect(result.skip).toBe(0)
      expect(result.take).toBe(5)
      expect(result.transactions.length).toBeLessThanOrEqual(5)
    })

    it('should apply skip parameter correctly', async () => {
      server.use(...successHandlers)

      const result1 = await transactionsService.getTransactions('acc-001', 0, 2)
      const result2 = await transactionsService.getTransactions('acc-001', 1, 2)

      expect(result1.skip).toBe(0)
      expect(result2.skip).toBe(1)
    })

    it('should support category filtering', async () => {
      server.use(...successHandlers)

      const result = await transactionsService.getTransactions(
        'acc-001',
        0,
        10,
        'Food & Dining'
      )

      expect(result.transactions).toBeDefined()
      // All returned transactions should be of the filtered category
      result.transactions.forEach((txn) => {
        expect(txn.category).toBe('Food & Dining')
      })
    })

    it('should return different categories correctly', async () => {
      server.use(...successHandlers)

      const foodResult = await transactionsService.getTransactions(
        'acc-001',
        0,
        10,
        'Food & Dining'
      )
      const utilityResult = await transactionsService.getTransactions(
        'acc-001',
        0,
        10,
        'Utilities'
      )

      // Each category should filter results
      if (foodResult.transactions.length > 0) {
        expect(foodResult.transactions[0].category).toBe('Food & Dining')
      }
      if (utilityResult.transactions.length > 0) {
        expect(utilityResult.transactions[0].category).toBe('Utilities')
      }
    })

    it('should include total count', async () => {
      server.use(...successHandlers)

      const result = await transactionsService.getTransactions('acc-001')

      expect(result.total).toBeGreaterThanOrEqual(result.transactions.length)
    })

    it('should handle default pagination parameters', async () => {
      server.use(...successHandlers)

      const result = await transactionsService.getTransactions('acc-001')

      expect(result.skip).toBe(0)
      expect(result.take).toBe(10)
    })

    it('should handle valid transaction types', async () => {
      server.use(...successHandlers)

      const result = await transactionsService.getTransactions('acc-001')

      result.transactions.forEach((txn) => {
        expect(['Debit', 'Credit']).toContain(txn.type)
      })
    })

    it('should handle valid transaction categories', async () => {
      server.use(...successHandlers)

      const validCategories = [
        'Groceries',
        'Utilities',
        'Entertainment',
        'Food & Dining',
        'Transport',
        'Salary',
        'Transfer',
        'Other',
      ]

      const result = await transactionsService.getTransactions('acc-001')

      result.transactions.forEach((txn) => {
        expect(validCategories).toContain(txn.category)
      })
    })

    it('should return transactions ordered by date', async () => {
      server.use(...successHandlers)

      const result = await transactionsService.getTransactions('acc-001')

      if (result.transactions.length > 1) {
        for (let i = 0; i < result.transactions.length - 1; i++) {
          const current = new Date(result.transactions[i].date).getTime()
          const next = new Date(result.transactions[i + 1].date).getTime()
          // Should be ordered descending (newest first)
          expect(current).toBeGreaterThanOrEqual(next)
        }
      }
    })
  })
})
