import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { AlertCircle, Loader2, ChevronLeft, ChevronRight, RefreshCw } from 'lucide-react'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../components/ui/table'
import { useToast } from '../hooks/useToast'
import AccountsService from '../services/accountsService'
import TransactionsService from '../services/transactionsService'
import type { AccountDto, TransactionDto, TransactionCategory, ApiError } from '../types'

/**
 * Category colors for badge styling
 * Maps transaction categories to Tailwind color classes
 */
const CATEGORY_COLORS: Record<TransactionCategory, { bg: string; text: string; badge: string }> = {
  'Groceries': { bg: 'bg-green-900/30', text: 'text-green-300', badge: 'bg-green-500' },
  'Utilities': { bg: 'bg-blue-900/30', text: 'text-blue-300', badge: 'bg-blue-500' },
  'Entertainment': { bg: 'bg-purple-900/30', text: 'text-purple-300', badge: 'bg-purple-500' },
  'Food & Dining': { bg: 'bg-orange-900/30', text: 'text-orange-300', badge: 'bg-orange-500' },
  'Transport': { bg: 'bg-cyan-900/30', text: 'text-cyan-300', badge: 'bg-cyan-500' },
  'Salary': { bg: 'bg-green-900/30', text: 'text-green-300', badge: 'bg-green-500' },
  'Transfer': { bg: 'bg-yellow-900/30', text: 'text-yellow-300', badge: 'bg-yellow-500' },
  'Other': { bg: 'bg-gray-900/30', text: 'text-gray-300', badge: 'bg-gray-500' },
}

const ALL_CATEGORIES: TransactionCategory[] = [
  'Groceries',
  'Utilities',
  'Entertainment',
  'Food & Dining',
  'Transport',
  'Salary',
  'Transfer',
  'Other',
]

const PAGE_SIZES = [10, 25, 50]

/**
 * TransactionsPage: Full transactions page implementation with table and filters
 * - Account selector dropdown (populated from accounts list)
 * - Calls getTransactions(accountId) on load and account change
 * - shadcn/ui Table with columns: Date, Description, Category, Amount
 * - Category badges (colored by category)
 * - Category filter selector (select from predefined categories)
 * - Pagination: prev/next buttons, page size selector (10/25/50)
 * - Loading state: table skeleton
 * - Empty state: "No transactions" message
 * - Dark theme ready
 */
function TransactionsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const toast = useToast()
  const [accounts, setAccounts] = useState<AccountDto[]>([])
  const [transactions, setTransactions] = useState<TransactionDto[]>([])
  const [selectedAccountId, setSelectedAccountId] = useState<string>('')
  const [selectedCategory, setSelectedCategory] = useState<TransactionCategory | 'All'>('All')
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

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
    try {
      const data = await AccountsService.getAccounts()
      setAccounts(data)

      // Set selected account from URL or use first account
      const accountIdFromUrl = searchParams.get('accountId')
      if (accountIdFromUrl && data.some(a => a.id === accountIdFromUrl)) {
        setSelectedAccountId(accountIdFromUrl)
      } else if (data.length > 0) {
        setSelectedAccountId(data[0].id)
      }
    } catch (err) {
      const apiErr = err as ApiError
      let errorMsg = 'Failed to load accounts'

      if (apiErr.response?.data?.message) {
        errorMsg = apiErr.response.data.message
      } else if (apiErr.message) {
        errorMsg = apiErr.message
      }

      setError(errorMsg)
      toast.error(errorMsg)
    }
  }

  /**
   * Load transactions when account, filters, or pagination changes
   */
  useEffect(() => {
    if (selectedAccountId) {
      loadTransactions()
    }
  }, [selectedAccountId, selectedCategory, currentPage, pageSize])

  /**
   * Load transactions from API
   */
  const loadTransactions = async () => {
    if (!selectedAccountId) return

    setIsLoading(true)
    setError(null)

    try {
      const skip = (currentPage - 1) * pageSize
      const category = selectedCategory !== 'All' ? selectedCategory : undefined

      const data = await TransactionsService.getTransactions(
        selectedAccountId,
        skip,
        pageSize,
        category
      )

      setTransactions(data.transactions)
      setTotal(data.total)
    } catch (err) {
      const apiErr = err as ApiError
      let errorMsg = 'Failed to load transactions'

      if (apiErr.response?.data?.message) {
        errorMsg = apiErr.response.data.message
      } else if (apiErr.message) {
        errorMsg = apiErr.message
      }

      setError(errorMsg)
      toast.error(errorMsg)
      setTransactions([])
    } finally {
      setIsLoading(false)
    }
  }

  /**
   * Handle account selection change
   */
  const handleAccountChange = (accountId: string) => {
    setSelectedAccountId(accountId)
    setCurrentPage(1)
    setSelectedCategory('All')
    setSearchParams({ accountId })
  }

  /**
   * Handle category filter change
   */
  const handleCategoryChange = (category: TransactionCategory | 'All') => {
    setSelectedCategory(category)
    setCurrentPage(1)
  }

  /**
   * Handle page size change
   */
  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize)
    setCurrentPage(1)
  }

  /**
   * Format currency with sign based on transaction type
   */
  const formatAmount = (amount: number, type: 'Debit' | 'Credit'): string => {
    const formatter = new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    })
    const sign = type === 'Credit' ? '+' : '-'
    return `${sign}${formatter.format(amount)}`
  }

  /**
   * Format transaction date
   */
  const formatDate = (isoString: string): string => {
    try {
      const date = new Date(isoString)
      const formatter = new Intl.DateTimeFormat('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
      })
      return formatter.format(date)
    } catch {
      return 'Unknown date'
    }
  }

  /**
   * Calculate pagination info
   */
  const totalPages = Math.ceil(total / pageSize)
  const startRow = total === 0 ? 0 : (currentPage - 1) * pageSize + 1
  const endRow = Math.min(currentPage * pageSize, total)

  /**
   * Skeleton row component for loading state
   */
  const SkeletonRow = () => (
    <TableRow className="animate-pulse">
      <TableCell className="h-12 bg-slate-800 rounded"></TableCell>
      <TableCell className="h-12 bg-slate-800 rounded"></TableCell>
      <TableCell className="h-12 bg-slate-800 rounded"></TableCell>
      <TableCell className="h-12 bg-slate-800 rounded"></TableCell>
    </TableRow>
  )

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-4xl font-bold text-white mb-2">Transactions</h1>
        <p className="text-slate-400">View and filter your account transactions</p>
      </div>

      {/* Error State */}
      {error && (
        <div className="flex items-start gap-3 p-4 bg-red-900/20 border border-red-700/30 rounded-lg">
          <AlertCircle className="h-5 w-5 text-red-500 mt-0.5 flex-shrink-0" />
          <div className="flex-1">
            <p className="text-red-300">{error}</p>
          </div>
          <Button
            onClick={() => loadTransactions()}
            variant="outline"
            size="sm"
            className="border-red-700/30 hover:bg-red-900/20 text-red-300 ml-4"
          >
            <RefreshCw className="h-4 w-4 mr-1" />
            Retry
          </Button>
        </div>
      )}

      {/* Filters Section */}
      <Card className="bg-slate-900 border-slate-700">
        <CardHeader>
          <CardTitle className="text-white">Filters</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Account Selector */}
          <div className="space-y-2">
            <label className="text-sm font-medium text-white">Account</label>
            <select
              value={selectedAccountId}
              onChange={(e) => handleAccountChange(e.target.value)}
              disabled={isLoading}
              className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-md text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {accounts.length === 0 ? (
                <option value="">No accounts available</option>
              ) : (
                accounts.map((account) => (
                  <option key={account.id} value={account.id}>
                    {account.name} - {new Intl.NumberFormat('en-US', {
                      style: 'currency',
                      currency: account.currency,
                    }).format(account.balance)}
                  </option>
                ))
              )}
            </select>
          </div>

          {/* Category Filter */}
          <div className="space-y-2">
            <label className="text-sm font-medium text-white">Category</label>
            <select
              value={selectedCategory}
              onChange={(e) => handleCategoryChange(e.target.value as TransactionCategory | 'All')}
              disabled={isLoading}
              className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-md text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <option value="All">All Categories</option>
              {ALL_CATEGORIES.map((category) => (
                <option key={category} value={category}>
                  {category}
                </option>
              ))}
            </select>
          </div>

          {/* Page Size Selector */}
          <div className="space-y-2">
            <label className="text-sm font-medium text-white">Rows per page</label>
            <select
              value={pageSize}
              onChange={(e) => handlePageSizeChange(parseInt(e.target.value))}
              disabled={isLoading}
              className="w-full px-3 py-2 bg-slate-800 border border-slate-700 rounded-md text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {PAGE_SIZES.map((size) => (
                <option key={size} value={size}>
                  {size} rows
                </option>
              ))}
            </select>
          </div>
        </CardContent>
      </Card>

      {/* Table Section */}
      <Card className="bg-slate-900 border-slate-700 overflow-hidden">
        <CardContent className="p-0">
          {isLoading ? (
            // Loading state: skeleton
            <div className="overflow-auto">
              <Table>
                <TableHeader className="bg-slate-800 border-slate-700">
                  <TableRow className="border-slate-700">
                    <TableHead className="text-white">Date</TableHead>
                    <TableHead className="text-white">Description</TableHead>
                    <TableHead className="text-white">Category</TableHead>
                    <TableHead className="text-right text-white">Amount</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {Array(5).fill(0).map((_, i) => (
                    <SkeletonRow key={i} />
                  ))}
                </TableBody>
              </Table>
            </div>
          ) : transactions.length === 0 ? (
            // Empty state
            <div className="p-8 text-center">
              <p className="text-slate-400 mb-2">No transactions found</p>
              <p className="text-sm text-slate-500">
                Try changing the filter or selecting a different account.
              </p>
            </div>
          ) : (
            // Table with transactions
            <div className="overflow-auto">
              <Table>
                <TableHeader className="bg-slate-800 border-slate-700">
                  <TableRow className="border-slate-700">
                    <TableHead className="text-white">Date</TableHead>
                    <TableHead className="text-white">Description</TableHead>
                    <TableHead className="text-white">Category</TableHead>
                    <TableHead className="text-right text-white">Amount</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {transactions.map((transaction) => {
                    const colors = CATEGORY_COLORS[transaction.category]
                    const isCredit = transaction.type === 'Credit'
                    const amountColor = isCredit ? 'text-green-400' : 'text-red-400'

                    return (
                      <TableRow key={transaction.id} className="border-slate-700 hover:bg-slate-800/50">
                        <TableCell className="text-slate-300">
                          {formatDate(transaction.date)}
                        </TableCell>
                        <TableCell className="text-slate-300">
                          {transaction.description}
                        </TableCell>
                        <TableCell>
                          <span className={`inline-block px-3 py-1 rounded-full text-xs font-semibold text-white ${colors.badge}`}>
                            {transaction.category}
                          </span>
                        </TableCell>
                        <TableCell className={`text-right font-semibold ${amountColor}`}>
                          {formatAmount(transaction.amount, transaction.type)}
                        </TableCell>
                      </TableRow>
                    )
                  })}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Pagination Section */}
      {transactions.length > 0 && (
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4 p-4 bg-slate-900 border border-slate-700 rounded-lg">
          {/* Info text */}
          <div className="text-sm text-slate-400">
            Showing {startRow} to {endRow} of {total} transactions
          </div>

          {/* Pagination controls */}
          <div className="flex items-center gap-2">
            <Button
              onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1 || isLoading}
              variant="outline"
              size="sm"
              className="border-slate-700 text-slate-400 hover:text-white hover:bg-slate-800"
            >
              <ChevronLeft className="h-4 w-4 mr-1" />
              Previous
            </Button>

            {/* Page indicator */}
            <div className="text-sm text-slate-400 px-4">
              Page {currentPage} of {totalPages}
            </div>

            <Button
              onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
              disabled={currentPage === totalPages || isLoading}
              variant="outline"
              size="sm"
              className="border-slate-700 text-slate-400 hover:text-white hover:bg-slate-800"
            >
              Next
              <ChevronRight className="h-4 w-4 ml-1" />
            </Button>
          </div>

          {/* Loading indicator */}
          {isLoading && (
            <div className="flex items-center gap-2 text-slate-400">
              <Loader2 className="h-4 w-4 animate-spin" />
              <span className="text-sm">Loading...</span>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

export default TransactionsPage
