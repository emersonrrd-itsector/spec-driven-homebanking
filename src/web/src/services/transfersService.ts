import api from './api'
import type { TransferRequest, TransferResponse } from '../types'

/**
 * TransfersService: Handles transfer-related API calls
 */
class TransfersService {
  /**
   * Create a transfer between accounts
   * @param fromAccountId Source account ID
   * @param toAccountId Destination account ID
   * @param amount Transfer amount (must be > 0.01)
   * @param description Optional description
   * @returns Transfer response with updated balances
   */
  async createTransfer(
    fromAccountId: string,
    toAccountId: string,
    amount: number,
    description?: string
  ): Promise<TransferResponse> {
    const payload: TransferRequest = {
      fromAccountId,
      toAccountId,
      amount,
      description,
    }

    const response = await api.post<TransferResponse>('/api/transfers', payload)
    return response.data
  }
}

export default new TransfersService()
