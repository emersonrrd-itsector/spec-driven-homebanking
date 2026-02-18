import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { AlertCircle, Loader2, RefreshCw } from 'lucide-react'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '../components/ui/form'
import { Input } from '../components/ui/input'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card'
import { useToast } from '../hooks/useToast'
import AccountsService from '../services/accountsService'
import TransfersService from '../services/transfersService'
import type { AccountDto, ApiError } from '../types'

/**
 * TransferPage: Full transfer form with validation and API integration
 * - Account selector dropdowns (from and to)
 * - Amount input with validation (> 0.01)
 * - Optional description textarea
 * - Client-side validation via React Hook Form
 * - From != To validation
 * - TransfersService integration for API calls
 * - Loading state during submission
 * - Error handling with retry capability
 * - Success redirect to /transactions
 * - Dark theme ready
 */
function TransferPage() {
  const navigate = useNavigate()
  const toast = useToast()
  const [accounts, setAccounts] = useState<AccountDto[]>([])
  const [isLoadingAccounts, setIsLoadingAccounts] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [accountsError, setAccountsError] = useState<string | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)

  // Initialize form with validation rules
  const form = useForm({
    defaultValues: {
      fromAccountId: '',
      toAccountId: '',
      amount: '',
      description: '',
    },
    mode: 'onBlur', // Validate on blur for better UX
  })

  /**
   * Load accounts on mount
   */
  useEffect(() => {
    loadAccounts()
  }, [])

  /**
   * Load accounts from API
   */
  const loadAccounts = async () => {
    setIsLoadingAccounts(true)
    setAccountsError(null)

    try {
      const data = await AccountsService.getAccounts()
      setAccounts(data)
      if (data.length > 0) {
        form.setValue('fromAccountId', data[0].id)
      }
    } catch (err) {
      const apiErr = err as ApiError
      const errorMsg = apiErr.response?.data?.message || apiErr.message || 'Failed to load accounts'
      setAccountsError(errorMsg)
    } finally {
      setIsLoadingAccounts(false)
    }
  }

  /**
   * Handle form submission
   * Calls TransfersService.createTransfer() and navigates to /transactions on success
   */
  const handleSubmit = async (data: {
    fromAccountId: string
    toAccountId: string
    amount: string
    description: string
  }) => {
    setSubmitError(null)
    setIsSubmitting(true)

    try {
      const amount = parseFloat(data.amount)

      // Call TransfersService to create the transfer
      const result = await TransfersService.createTransfer(
        data.fromAccountId,
        data.toAccountId,
        amount,
        data.description || undefined
      )

      // Show success toast
      toast.success(`Transfer of $${amount.toFixed(2)} completed successfully!`)
      console.log('Transfer successful:', result)

      // Navigate to transactions page with the source account selected
      navigate(`/transactions?accountId=${data.fromAccountId}`)
    } catch (error) {
      // Handle API errors and network errors
      const apiErr = error as ApiError

      // Extract error message with user-friendly formatting
      let errorMessage = 'Transfer failed. Please try again.'

      if (apiErr.response?.data?.code === 'InsufficientBalance') {
        // Format insufficient balance error with details
        const available = apiErr.response.data.details?.available
        const requested = apiErr.response.data.details?.requested
        if (available !== undefined && requested !== undefined) {
          errorMessage = `Insufficient balance. Available: $${available}, Requested: $${requested}`
        } else {
          errorMessage = 'Insufficient balance in source account'
        }
      } else if (apiErr.response?.data?.message) {
        // Use API error message
        errorMessage = apiErr.response.data.message
      } else if (apiErr.message) {
        // Network or other error
        errorMessage = apiErr.message
      }

      // Show error toast
      toast.error(errorMessage)
      setSubmitError(errorMessage)
      console.error('Transfer error:', error)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-4xl font-bold text-white mb-2">Transfer Money</h1>
        <p className="text-slate-400">Transfer funds between your accounts</p>
      </div>

      {/* Accounts Loading Error */}
      {accountsError && (
        <div className="flex items-start gap-3 p-4 bg-red-900/20 border border-red-700/30 rounded-lg">
          <AlertCircle className="h-5 w-5 text-red-500 mt-0.5 flex-shrink-0" />
          <div className="flex-1">
            <p className="text-red-300">{accountsError}</p>
          </div>
          <Button
            onClick={loadAccounts}
            variant="outline"
            size="sm"
            className="border-red-700/30 hover:bg-red-900/20 text-red-300 ml-4"
          >
            <RefreshCw className="h-4 w-4 mr-1" />
            Retry
          </Button>
        </div>
      )}

      {/* Transfer Form Container */}
      <Card className="bg-slate-900 border-slate-700">
        <CardHeader>
          <CardTitle className="text-white">Transfer Details</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoadingAccounts ? (
            // Loading state
            <div className="flex items-center justify-center py-8">
              <Loader2 className="h-6 w-6 animate-spin text-slate-400 mr-2" />
              <p className="text-slate-400">Loading accounts...</p>
            </div>
          ) : accounts.length === 0 ? (
            // No accounts state
            <div className="text-center py-8">
              <p className="text-slate-400 mb-2">No accounts available</p>
              <p className="text-sm text-slate-500">
                You need at least one account to make a transfer.
              </p>
            </div>
          ) : (
            // Form
            <>
              {/* Submit Error Message */}
              {submitError && (
                <div className="mb-6 p-3 bg-red-900/20 border border-red-700/30 rounded text-sm text-red-300">
                  {submitError}
                </div>
              )}

              <Form {...form}>
                <form
                  onSubmit={form.handleSubmit(handleSubmit)}
                  className="space-y-6"
                >
                  {/* From Account Field */}
                  <FormField
                    control={form.control}
                    name="fromAccountId"
                    rules={{
                      required: 'Source account is required',
                    }}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel className="text-slate-200">From Account</FormLabel>
                        <FormControl>
                          <select
                            {...field}
                            disabled={isSubmitting}
                            className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-md text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <option value="">Select source account...</option>
                            {accounts.map((account) => (
                              <option key={account.id} value={account.id}>
                                {account.name} - {new Intl.NumberFormat('en-US', {
                                  style: 'currency',
                                  currency: account.currency,
                                }).format(account.balance)}
                              </option>
                            ))}
                          </select>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* To Account Field */}
                  <FormField
                    control={form.control}
                    name="toAccountId"
                    rules={{
                      required: 'Destination account is required',
                      validate: (value) => {
                        if (value === form.watch('fromAccountId')) {
                          return 'Destination account must be different from source account'
                        }
                        return true
                      },
                    }}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel className="text-slate-200">To Account</FormLabel>
                        <FormControl>
                          <select
                            {...field}
                            disabled={isSubmitting}
                            className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-md text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <option value="">Select destination account...</option>
                            {accounts.map((account) => (
                              <option key={account.id} value={account.id}>
                                {account.name} - {new Intl.NumberFormat('en-US', {
                                  style: 'currency',
                                  currency: account.currency,
                                }).format(account.balance)}
                              </option>
                            ))}
                          </select>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Amount Field */}
                  <FormField
                    control={form.control}
                    name="amount"
                    rules={{
                      required: 'Amount is required',
                      validate: (value) => {
                        const amount = parseFloat(value)
                        if (isNaN(amount)) {
                          return 'Amount must be a valid number'
                        }
                        if (amount <= 0.01) {
                          return 'Amount must be greater than $0.01'
                        }
                        return true
                      },
                    }}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel className="text-slate-200">Amount (USD)</FormLabel>
                        <FormControl>
                          <Input
                            {...field}
                            type="number"
                            placeholder="0.00"
                            step="0.01"
                            min="0"
                            disabled={isSubmitting}
                            className="bg-slate-800 border-slate-700 text-white placeholder:text-slate-500"
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Description Field */}
                  <FormField
                    control={form.control}
                    name="description"
                    rules={{
                      maxLength: {
                        value: 500,
                        message: 'Description cannot exceed 500 characters',
                      },
                    }}
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel className="text-slate-200">Description (Optional)</FormLabel>
                        <FormControl>
                          <textarea
                            {...field}
                            placeholder="Payment description (optional)"
                            disabled={isSubmitting}
                            maxLength={500}
                            rows={4}
                            className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-md text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed resize-none"
                          />
                        </FormControl>
                        <div className="text-xs text-slate-400">
                          {field.value?.length || 0} / 500 characters
                        </div>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Submit Button */}
                  <Button
                    type="submit"
                    disabled={isSubmitting || !form.formState.isValid || isLoadingAccounts}
                    className="w-full bg-blue-600 hover:bg-blue-700 text-white"
                  >
                    {isSubmitting ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Transferring...
                      </>
                    ) : (
                      'Transfer'
                    )}
                  </Button>
                </form>
              </Form>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

export default TransferPage
