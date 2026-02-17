import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { AlertCircle, Loader2, RefreshCw } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card'
import { Button } from '../components/ui/button'
import AccountsService from '../services/accountsService'
import AuthService from '../services/authService'
import type { AccountDto, ApiError } from '../types'

/**
 * DashboardPage: Full dashboard implementation with account cards grid
 * - Welcome message with logged-in user name
 * - Responsive card grid layout (1 column on mobile, 2 on tablet, 3 on desktop)
 * - Account cards showing: name, balance (formatted currency), last updated
 * - Card click navigation to /transactions
 * - Loading state with skeleton cards (Tailwind pulse effect)
 * - Error state with retry button
 * - Dark theme ready
 */
function DashboardPage() {
  const navigate = useNavigate()
  const [accounts, setAccounts] = useState<AccountDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const currentUser = AuthService.getCurrentUser()

  /**
   * Load accounts from API
   */
  const loadAccounts = async () => {
    setIsLoading(true)
    setError(null)

    try {
      const data = await AccountsService.getAccounts()
      setAccounts(data)
    } catch (err) {
      const apiErr = err as ApiError
      if (apiErr.response?.data?.message) {
        setError(apiErr.response.data.message)
      } else if (apiErr.message) {
        setError(apiErr.message)
      } else {
        setError('Failed to load accounts. Please try again.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  /**
   * Load accounts on component mount
   */
  useEffect(() => {
    loadAccounts()
  }, [])

  /**
   * Handle account card click - navigate to transactions page
   */
  const handleAccountClick = (accountId: string) => {
    navigate(`/transactions?accountId=${accountId}`)
  }

  /**
   * Format number as currency (USD)
   */
  const formatCurrency = (amount: number, currency: string = 'USD'): string => {
    const formatter = new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    })
    return formatter.format(amount)
  }

  /**
   * Format timestamp to readable format
   */
  const formatDate = (isoString: string): string => {
    try {
      const date = new Date(isoString)
      const formatter = new Intl.DateTimeFormat('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
        hour12: true,
      })
      return formatter.format(date)
    } catch {
      return 'Unknown date'
    }
  }

  /**
   * Skeleton card component for loading state
   */
  const SkeletonCard = () => (
    <Card className="bg-slate-900 border-slate-700 cursor-pointer animate-pulse">
      <CardHeader>
        <div className="h-6 bg-slate-800 rounded w-3/4 mb-2"></div>
        <div className="h-4 bg-slate-800 rounded w-1/2"></div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="h-8 bg-slate-800 rounded w-2/3"></div>
        <div className="h-4 bg-slate-800 rounded w-1/2"></div>
      </CardContent>
    </Card>
  )

  return (
    <div className="space-y-8">
      {/* Welcome Section */}
      <div>
        <h1 className="text-4xl font-bold text-white mb-2">
          {currentUser
            ? `Welcome, ${currentUser.email.split('@')[0]}!`
            : 'Welcome!'}
        </h1>
        <p className="text-slate-400">
          {currentUser ? `Logged in as ${currentUser.email}` : 'Your accounts overview'}
        </p>
      </div>

      {/* Error State */}
      {error && (
        <div className="flex items-start gap-3 p-4 bg-red-900/20 border border-red-700/30 rounded-lg">
          <AlertCircle className="h-5 w-5 text-red-500 mt-0.5 flex-shrink-0" />
          <div className="flex-1">
            <p className="text-red-300">{error}</p>
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

      {/* Account Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {isLoading ? (
          // Show skeleton cards while loading
          <>
            <SkeletonCard />
            <SkeletonCard />
            <SkeletonCard />
          </>
        ) : accounts.length > 0 ? (
          // Show account cards
          accounts.map((account) => (
            <Card
              key={account.id}
              onClick={() => handleAccountClick(account.id)}
              className="bg-slate-900 border-slate-700 hover:border-slate-600 hover:shadow-lg hover:shadow-slate-900/50 transition-all duration-200 cursor-pointer hover:scale-105"
            >
              <CardHeader>
                <CardTitle className="text-white text-lg">
                  {account.name}
                </CardTitle>
                <p className="text-sm text-slate-400">
                  Account #{account.id.slice(-4).toUpperCase()}
                </p>
              </CardHeader>
              <CardContent className="space-y-4">
                {/* Balance */}
                <div>
                  <p className="text-sm text-slate-400 mb-1">Balance</p>
                  <p className="text-2xl font-bold text-green-400">
                    {formatCurrency(account.balance, account.currency)}
                  </p>
                </div>

                {/* Currency and Last Updated */}
                <div className="flex justify-between items-end text-sm text-slate-500">
                  <span>{account.currency}</span>
                  <span>
                    Updated: {formatDate(account.lastUpdated)}
                  </span>
                </div>

                {/* Click hint */}
                <p className="text-xs text-slate-500 pt-2 italic">
                  Click to view transactions
                </p>
              </CardContent>
            </Card>
          ))
        ) : (
          // Show empty state
          <div className="col-span-full">
            <Card className="bg-slate-900 border-slate-700">
              <CardContent className="pt-6 text-center">
                <p className="text-slate-400 mb-2">No accounts found</p>
                <p className="text-sm text-slate-500">
                  Please contact support or try again later.
                </p>
              </CardContent>
            </Card>
          </div>
        )}
      </div>

      {/* Loading indicator text */}
      {isLoading && (
        <div className="flex items-center justify-center gap-2 text-slate-400">
          <Loader2 className="h-4 w-4 animate-spin" />
          <span className="text-sm">Loading your accounts...</span>
        </div>
      )}
    </div>
  )
}

export default DashboardPage
