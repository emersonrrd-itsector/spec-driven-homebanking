import { describe, it, expect } from 'vitest'
import { server } from '../tests/setup'
import accountsService from './accountsService'
import { successHandlers, errorHandlers } from '../tests/mocks/handlers'

describe('AccountsService', () => {
  describe('getAccounts', () => {
    it('should fetch all accounts', async () => {
      server.use(...successHandlers)

      const accounts = await accountsService.getAccounts()

      expect(accounts).toBeDefined()
      expect(accounts.length).toBe(2)
      expect(accounts[0].id).toBe('acc-001')
      expect(accounts[0].name).toBe('Checking')
      expect(accounts[0].balance).toBe(5000)
      expect(accounts[1].id).toBe('acc-002')
      expect(accounts[1].name).toBe('Savings')
      expect(accounts[1].balance).toBe(10000)
    })

    it('should return accounts with correct currency', async () => {
      server.use(...successHandlers)

      const accounts = await accountsService.getAccounts()

      expect(accounts[0].currency).toBe('USD')
      expect(accounts[1].currency).toBe('USD')
    })

    it('should return accounts with lastUpdated timestamp', async () => {
      server.use(...successHandlers)

      const accounts = await accountsService.getAccounts()

      expect(accounts[0].lastUpdated).toBeDefined()
      expect(accounts[1].lastUpdated).toBeDefined()
    })

    it('should throw error on server error', async () => {
      server.use(errorHandlers.serverError)

      try {
        await accountsService.getAccounts()
        expect.fail('Should have thrown error')
      } catch (error: any) {
        expect(error.response?.status).toBe(500)
        expect(error.response?.data.code).toBe('InternalServerError')
      }
    })
  })

  describe('getAccount', () => {
    it('should fetch specific account by id', async () => {
      server.use(...successHandlers)

      const account = await accountsService.getAccount('acc-001')

      expect(account).toBeDefined()
      expect(account.id).toBe('acc-001')
      expect(account.name).toBe('Checking')
      expect(account.balance).toBe(5000)
    })

    it('should return account with all required fields', async () => {
      server.use(...successHandlers)

      const account = await accountsService.getAccount('acc-001')

      expect(account.id).toBeDefined()
      expect(account.name).toBeDefined()
      expect(account.balance).toBeDefined()
      expect(account.currency).toBeDefined()
      expect(account.lastUpdated).toBeDefined()
    })

    it('should throw 404 error for non-existent account', async () => {
      server.use(...successHandlers)

      try {
        await accountsService.getAccount('acc-999')
        expect.fail('Should have thrown error')
      } catch (error: any) {
        expect(error.response?.status).toBe(404)
        expect(error.response?.data.code).toBe('NotFound')
      }
    })

    it('should throw error when requesting different accounts', async () => {
      server.use(...successHandlers)

      const account1 = await accountsService.getAccount('acc-001')
      const account2 = await accountsService.getAccount('acc-002')

      expect(account1.id).toBe('acc-001')
      expect(account2.id).toBe('acc-002')
      expect(account1.name).not.toBe(account2.name)
    })
  })
})
