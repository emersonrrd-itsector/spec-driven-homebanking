import { describe, it, expect, beforeEach, vi } from 'vitest'
import { render, screen, waitFor } from '../tests/test-utils'
import userEvent from '@testing-library/user-event'
import DashboardPage from './DashboardPage'
import AccountsService from '../services/accountsService'
import AuthService from '../services/authService'
import type { AccountDto, ApiError } from '../types'

// Mock dependencies
vi.mock('../services/accountsService')
vi.mock('../services/authService')
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => vi.fn(),
  }
})

// Mock account data
const mockAccounts: AccountDto[] = [
  {
    id: 'acc_0001',
    name: 'Checking',
    balance: 5000.0,
    currency: 'USD',
    lastUpdated: '2026-02-17T12:00:00Z',
  },
  {
    id: 'acc_0002',
    name: 'Savings',
    balance: 10000.5,
    currency: 'USD',
    lastUpdated: '2026-02-17T11:30:00Z',
  },
  {
    id: 'acc_0003',
    name: 'Business',
    balance: 25000.0,
    currency: 'USD',
    lastUpdated: '2026-02-17T10:15:00Z',
  },
]

const mockUser = {
  id: 'usr_001',
  email: 'admin@homebank.local',
}

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('Component Rendering', () => {
    it('should render the dashboard page with welcome message', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText(/Welcome, admin!/i)).toBeInTheDocument()
      })
    })

    it('should display user email in welcome message', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText(/Logged in as admin@homebank.local/i)).toBeInTheDocument()
      })
    })

    it('should show generic welcome message when user is not available', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(null)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue([])

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText(/^Welcome!$/i)).toBeInTheDocument()
      })
    })
  })

  describe('Account Cards Rendering', () => {
    it('should render account cards for each account', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText('Checking')).toBeInTheDocument()
        expect(screen.getByText('Savings')).toBeInTheDocument()
        expect(screen.getByText('Business')).toBeInTheDocument()
      })
    })

    it('should display formatted currency balance on cards', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText('$5,000.00')).toBeInTheDocument()
        expect(screen.getByText('$10,000.50')).toBeInTheDocument()
        expect(screen.getByText('$25,000.00')).toBeInTheDocument()
      })
    })

    it('should display account currency on cards', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        const currencyElements = screen.getAllByText('USD')
        expect(currencyElements.length).toBeGreaterThanOrEqual(3)
      })
    })

    it('should display last updated timestamp on cards', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        const dateElements = screen.getAllByText(/2026/i)
        expect(dateElements.length).toBeGreaterThanOrEqual(3)
      })
    })

    it('should display account numbers on cards', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText(/Account #0001/i)).toBeInTheDocument()
        expect(screen.getByText(/Account #0002/i)).toBeInTheDocument()
        expect(screen.getByText(/Account #0003/i)).toBeInTheDocument()
      })
    })

    it('should use responsive grid layout classes', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      const { container } = render(
        <DashboardPage />
      )

      await waitFor(() => {
        const gridContainer = container.querySelector(
          '.grid'
        ) as HTMLElement
        expect(gridContainer).toHaveClass('grid-cols-1')
        expect(gridContainer).toHaveClass('md:grid-cols-2')
        expect(gridContainer).toHaveClass('lg:grid-cols-3')
      })
    })
  })

  describe('Loading State', () => {
    it('should display loading message on initial load', () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockImplementation(
        () =>
          new Promise((resolve) => {
            setTimeout(() => resolve(mockAccounts), 100)
          })
      )

      render(
        <DashboardPage />
      )

      expect(screen.getByText(/Loading your accounts.../i)).toBeInTheDocument()
    })

    it('should show skeleton cards while loading', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockImplementation(
        () =>
          new Promise((resolve) => {
            setTimeout(() => resolve(mockAccounts), 100)
          })
      )

      const { container } = render(
        <DashboardPage />
      )

      // Check for skeleton cards (pulse animation elements)
      await waitFor(() => {
        const pulseElements = container.querySelectorAll('.animate-pulse')
        expect(pulseElements.length).toBeGreaterThan(0)
      })
    })

    it('should call getAccounts on component mount', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(AccountsService.getAccounts).toHaveBeenCalledTimes(1)
      })
    })

    it('should hide loading message after accounts load', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(
          screen.queryByText(/Loading your accounts.../i)
        ).not.toBeInTheDocument()
      })
    })
  })

  describe('Error State', () => {
    it('should display error message on API failure', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      const errorMessage = 'Failed to load accounts'
      vi.mocked(AccountsService.getAccounts).mockRejectedValue(
        new Error(errorMessage)
      )

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText(/Failed to load accounts/i)).toBeInTheDocument()
      })
    })

    it('should display API error message from structured response', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      const apiError = new Error('Unauthorized') as ApiError
      apiError.response = {
        status: 401,
        data: {
          code: 'Unauthorized',
          message: 'You are not authorized to access this resource',
          timestamp: new Date().toISOString(),
        },
      }
      vi.mocked(AccountsService.getAccounts).mockRejectedValue(apiError)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(
          screen.getByText(/You are not authorized to access this resource/i)
        ).toBeInTheDocument()
      })
    })

    it('should show retry button on error', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockRejectedValue(
        new Error('Network error')
      )

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /Retry/i })).toBeInTheDocument()
      })
    })

    it('should retry loading accounts when retry button is clicked', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts)
        .mockRejectedValueOnce(new Error('Network error'))
        .mockResolvedValueOnce(mockAccounts)

      const { rerender } = render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText(/Network error/i)).toBeInTheDocument()
      })

      const retryButton = screen.getByRole('button', { name: /Retry/i })
      await userEvent.click(retryButton)

      // Rerender after retry
      rerender(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText('Checking')).toBeInTheDocument()
      })
    })

    it('should show alert icon with error message', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockRejectedValue(
        new Error('API error')
      )

      const { container } = render(
        <DashboardPage />
      )

      await waitFor(() => {
        // Check for AlertCircle icon (lucide-react)
        const alertIcon = container.querySelector('svg')
        expect(alertIcon).toBeInTheDocument()
      })
    })
  })

  describe('Empty State', () => {
    it('should display empty state message when no accounts exist', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue([])

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText(/No accounts found/i)).toBeInTheDocument()
      })
    })

    it('should display helpful message in empty state', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue([])

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(
          screen.getByText(/Please contact support or try again later/i)
        ).toBeInTheDocument()
      })
    })
  })

  describe('Card Interactions', () => {
    it('should display click hint on account cards', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getAllByText(/Click to view transactions/i).length).toBe(
          mockAccounts.length
        )
      })
    })

    it('should have hover effects on cards', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      const { container } = render(
        <DashboardPage />
      )

      await waitFor(() => {
        const cards = container.querySelectorAll('[class*="hover"]')
        expect(cards.length).toBeGreaterThan(0)
      })
    })

    it('should have cursor-pointer class on cards', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      const { container } = render(
        <DashboardPage />
      )

      await waitFor(() => {
        const cards = container.querySelectorAll('.cursor-pointer')
        expect(cards.length).toBe(mockAccounts.length)
      })
    })
  })

  describe('Formatting', () => {
    it('should format balance with correct decimal places', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue([
        {
          id: 'acc_test',
          name: 'Test Account',
          balance: 1234.567,
          currency: 'USD',
          lastUpdated: '2026-02-17T12:00:00Z',
        },
      ])

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText('$1,234.57')).toBeInTheDocument()
      })
    })

    it('should format zero balance correctly', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue([
        {
          id: 'acc_zero',
          name: 'Zero Account',
          balance: 0,
          currency: 'USD',
          lastUpdated: '2026-02-17T12:00:00Z',
        },
      ])

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText('$0.00')).toBeInTheDocument()
      })
    })

    it('should format large balance correctly', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue([
        {
          id: 'acc_large',
          name: 'Large Account',
          balance: 1000000.0,
          currency: 'USD',
          lastUpdated: '2026-02-17T12:00:00Z',
        },
      ])

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText('$1,000,000.00')).toBeInTheDocument()
      })
    })

    it('should format date correctly in lastUpdated', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue([
        {
          id: 'acc_date_test',
          name: 'Date Test',
          balance: 1000,
          currency: 'USD',
          lastUpdated: '2026-02-17T14:30:00Z',
        },
      ])

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        // Check for formatted date (includes month, day, year, time)
        expect(screen.getByText(/Feb 17, 2026/i)).toBeInTheDocument()
      })
    })

    it('should handle invalid date gracefully', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue([
        {
          id: 'acc_invalid_date',
          name: 'Invalid Date Test',
          balance: 1000,
          currency: 'USD',
          lastUpdated: 'invalid-date',
        },
      ])

      render(
        <DashboardPage />
      )

      await waitFor(() => {
        expect(screen.getByText(/Unknown date/i)).toBeInTheDocument()
      })
    })
  })

  describe('Dark Theme', () => {
    it('should use dark theme classes on cards', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      const { container } = render(
        <DashboardPage />
      )

      await waitFor(() => {
        const cards = container.querySelectorAll('.bg-slate-900')
        expect(cards.length).toBeGreaterThan(0)
      })
    })

    it('should use dark theme text colors', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      const { container } = render(
        <DashboardPage />
      )

      await waitFor(() => {
        const textElements = container.querySelectorAll('.text-white')
        expect(textElements.length).toBeGreaterThan(0)
      })
    })

    it('should use slate border colors for dark theme', async () => {
      vi.mocked(AuthService.getCurrentUser).mockReturnValue(mockUser)
      vi.mocked(AccountsService.getAccounts).mockResolvedValue(mockAccounts)

      const { container } = render(
        <DashboardPage />
      )

      await waitFor(() => {
        const borderElements = container.querySelectorAll('.border-slate-700')
        expect(borderElements.length).toBeGreaterThan(0)
      })
    })
  })
})
