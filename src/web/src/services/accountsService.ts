import api from './api'
import type { AccountDto, AccountsResponse } from '../types'

/**
 * AccountsService: Handles account-related API calls
 */
class AccountsService {
  /**
   * Get all accounts for the current user
   * @returns List of accounts with balances
   */
  async getAccounts(): Promise<AccountDto[]> {
    const response = await api.get<AccountsResponse>('/api/accounts')
    return response.data.accounts
  }

  /**
   * Get a specific account by ID
   * @param id Account ID
   * @returns Account details
   */
  async getAccount(id: string): Promise<AccountDto> {
    const response = await api.get<AccountDto>(`/api/accounts/${id}`)
    return response.data
  }
}

export default new AccountsService()
