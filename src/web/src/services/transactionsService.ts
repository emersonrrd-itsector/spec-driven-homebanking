import api from './api'
import type { TransactionsResponse, TransactionCategory } from '../types'

/**
 * TransactionsService: Handles transaction-related API calls
 */
class TransactionsService {
  /**
   * Get paginated transactions for an account
   * @param accountId Account ID (required)
   * @param skip Number of records to skip (default: 0)
   * @param take Number of records to take (default: 10)
   * @param category Optional category filter
   * @returns Paginated transactions with total count
   */
  async getTransactions(
    accountId: string,
    skip: number = 0,
    take: number = 10,
    category?: TransactionCategory
  ): Promise<TransactionsResponse> {
    const params = new URLSearchParams({
      accountId,
      skip: skip.toString(),
      take: take.toString(),
    })

    if (category) {
      params.append('category', category)
    }

    const response = await api.get<TransactionsResponse>(
      `/api/transactions?${params.toString()}`
    )
    return response.data
  }
}

export default new TransactionsService()
