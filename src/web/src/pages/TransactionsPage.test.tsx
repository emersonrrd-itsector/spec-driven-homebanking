import { describe, it, expect, beforeEach, vi } from 'vitest'
import { render, screen, fireEvent, waitFor } from '../tests/test-utils'
import TransactionsPage from './TransactionsPage'
import AccountsService from '../services/accountsService'
import TransactionsService from '../services/transactionsService'
import type { AccountDto, TransactionDto } from '../types'

// Mock the services
vi.mock('../services/accountsService')
vi.mock('../services/transactionsService')

// Sample test data
const MOCK_ACCOUNTS: AccountDto[] = [
  {
    id: 'acc_001',
    name: 'Checking',
    balance: 5000.00,
    currency: 'USD',
    lastUpdated: '2026-02-17T12:00:00Z',
  },
  {
    id: 'acc_002',
    name: 'Savings',
    balance: 10000.00,
    currency: 'USD',
    lastUpdated: '2026-02-17T12:00:00Z',
  },
]

const MOCK_TRANSACTIONS: TransactionDto[] = [
  {
    id: 'txn_001',
    accountId: 'acc_001',
    amount: 50.00,
    type: 'Debit',
    date: '2026-02-16T10:30:00Z',
    description: 'Coffee Shop',
    category: 'Food & Dining',
  },
  {
    id: 'txn_002',
    accountId: 'acc_001',
    amount: 120.00,
    type: 'Debit',
    date: '2026-02-15T14:20:00Z',
    description: 'Electric Bill',
    category: 'Utilities',
  },
  {
    id: 'txn_003',
    accountId: 'acc_001',
    amount: 2000.00,
    type: 'Credit',
    date: '2026-02-14T09:00:00Z',
    description: 'Salary Deposit',
    category: 'Salary',
  },
]

describe('TransactionsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Setup default mock implementations
    vi.mocked(AccountsService.getAccounts).mockResolvedValue(MOCK_ACCOUNTS)
    vi.mocked(TransactionsService.getTransactions).mockResolvedValue({
      transactions: MOCK_TRANSACTIONS,
      total: 3,
      skip: 0,
      take: 10,
    })
  })

  /**
   * Test component rendering
   */
  it('should render transactions page with header and filters', async () => {
    render(
      
        <TransactionsPage />
      
    )

    expect(screen.getByText('Transactions')).toBeInTheDocument()
    expect(screen.getByText('View and filter your account transactions')).toBeInTheDocument()
    expect(screen.getByLabelText('Account')).toBeInTheDocument()
    expect(screen.getByLabelText('Category')).toBeInTheDocument()
    expect(screen.getByLabelText('Rows per page')).toBeInTheDocument()
  })

  /**
   * Test loading accounts on mount
   */
  it('should load accounts on mount', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(AccountsService.getAccounts).toHaveBeenCalledTimes(1)
    })
  })

  /**
   * Test account selector population
   */
  it('should populate account selector with accounts', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      const accountSelect = screen.getByLabelText('Account') as HTMLSelectElement
      expect(accountSelect.value).toBe('acc_001')
      expect(screen.getByText('Checking - $5,000.00')).toBeInTheDocument()
      expect(screen.getByText('Savings - $10,000.00')).toBeInTheDocument()
    })
  })

  /**
   * Test loading transactions on mount
   */
  it('should load transactions on mount with default account', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(TransactionsService.getTransactions).toHaveBeenCalledWith(
        'acc_001',
        0,
        10,
        undefined
      )
    })
  })

  /**
   * Test rendering transaction table
   */
  it('should render transaction table with transactions', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(screen.getByText('Coffee Shop')).toBeInTheDocument()
      expect(screen.getByText('Electric Bill')).toBeInTheDocument()
      expect(screen.getByText('Salary Deposit')).toBeInTheDocument()
    })
  })

  /**
   * Test table column headers
   */
  it('should render table with correct column headers', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(screen.getByRole('columnheader', { name: 'Date' })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: 'Description' })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: 'Category' })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: 'Amount' })).toBeInTheDocument()
    })
  })

  /**
   * Test category badges rendering
   */
  it('should render category badges for transactions', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(screen.getByText('Food & Dining')).toBeInTheDocument()
      expect(screen.getByText('Utilities')).toBeInTheDocument()
      expect(screen.getByText('Salary')).toBeInTheDocument()
    })
  })

  /**
   * Test amount formatting
   */
  it('should format amounts correctly with +/- sign and currency', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      // Debit amounts should have minus sign
      expect(screen.getByText('-$50.00')).toBeInTheDocument()
      // Credit amounts should have plus sign
      expect(screen.getByText('+$2,000.00')).toBeInTheDocument()
    })
  })

  /**
   * Test date formatting
   */
  it('should format transaction dates correctly', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      // Check that dates are formatted (specific format may vary)
      const dateElements = screen.getAllByText(/Feb/)
      expect(dateElements.length).toBeGreaterThan(0)
    })
  })

  /**
   * Test account selection change
   */
  it('should load transactions for different account when selected', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(AccountsService.getAccounts).toHaveBeenCalled()
    })

    const accountSelect = screen.getByLabelText('Account') as HTMLSelectElement
    fireEvent.change(accountSelect, { target: { value: 'acc_002' } })

    await waitFor(() => {
      expect(TransactionsService.getTransactions).toHaveBeenCalledWith(
        'acc_002',
        0,
        10,
        undefined
      )
    })
  })

  /**
   * Test category filter
   */
  it('should filter transactions by category', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(TransactionsService.getTransactions).toHaveBeenCalled()
    })

    vi.clearAllMocks()
    vi.mocked(TransactionsService.getTransactions).mockResolvedValue({
      transactions: [MOCK_TRANSACTIONS[0]], // Only Food & Dining
      total: 1,
      skip: 0,
      take: 10,
    })

    const categorySelect = screen.getByLabelText('Category') as HTMLSelectElement
    fireEvent.change(categorySelect, { target: { value: 'Food & Dining' } })

    await waitFor(() => {
      expect(TransactionsService.getTransactions).toHaveBeenCalledWith(
        'acc_001',
        0,
        10,
        'Food & Dining'
      )
    })
  })

  /**
   * Test pagination - next button
   */
  it('should load next page when next button is clicked', async () => {
    vi.mocked(TransactionsService.getTransactions).mockResolvedValue({
      transactions: MOCK_TRANSACTIONS.slice(0, 10),
      total: 15,
      skip: 0,
      take: 10,
    })

    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(screen.getByText('Page 1 of 2')).toBeInTheDocument()
    })

    const nextButton = screen.getByRole('button', { name: /Next/ })
    fireEvent.click(nextButton)

    await waitFor(() => {
      expect(TransactionsService.getTransactions).toHaveBeenCalledWith(
        'acc_001',
        10,
        10,
        undefined
      )
    })
  })

  /**
   * Test pagination - previous button
   */
  it('should disable previous button on first page', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      const prevButton = screen.getByRole('button', { name: /Previous/ })
      expect(prevButton).toBeDisabled()
    })
  })

  /**
   * Test page size change
   */
  it('should change page size when selected', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(TransactionsService.getTransactions).toHaveBeenCalled()
    })

    vi.clearAllMocks()
    vi.mocked(TransactionsService.getTransactions).mockResolvedValue({
      transactions: MOCK_TRANSACTIONS,
      total: 50,
      skip: 0,
      take: 25,
    })

    const pageSizeSelect = screen.getByLabelText('Rows per page') as HTMLSelectElement
    fireEvent.change(pageSizeSelect, { target: { value: '25' } })

    await waitFor(() => {
      expect(TransactionsService.getTransactions).toHaveBeenCalledWith(
        'acc_001',
        0,
        25,
        undefined
      )
    })
  })

  /**
   * Test loading state with skeleton
   */
  it.skip('should show loading skeleton while fetching transactions', async () => {
    vi.mocked(TransactionsService.getTransactions).mockImplementationOnce(
      () => new Promise(resolve => setTimeout(() => resolve({
        transactions: MOCK_TRANSACTIONS,
        total: 3,
        skip: 0,
        take: 10,
      }), 100))
    )

    render(
      
        <TransactionsPage />
      
    )

    // Initially should show loading
    await waitFor(() => {
      expect(screen.getByText('Loading...')).toBeInTheDocument()
    })
  })

  /**
   * Test empty state
   */
  it('should show empty state when no transactions exist', async () => {
    vi.mocked(TransactionsService.getTransactions).mockResolvedValue({
      transactions: [],
      total: 0,
      skip: 0,
      take: 10,
    })

    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(screen.getByText('No transactions found')).toBeInTheDocument()
      expect(screen.getByText('Try changing the filter or selecting a different account.')).toBeInTheDocument()
    })
  })

  /**
   * Test error state
   */
  it('should show error message when API call fails', async () => {
    const errorMsg = 'Failed to load transactions'
    vi.mocked(AccountsService.getAccounts).mockRejectedValueOnce(
      new Error(errorMsg)
    )

    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(screen.getByText(errorMsg)).toBeInTheDocument()
    })
  })

  /**
   * Test retry button
   */
  it.skip('should retry loading when retry button is clicked', async () => {
    vi.mocked(AccountsService.getAccounts)
      .mockRejectedValueOnce(new Error('Network error'))
      .mockResolvedValueOnce(MOCK_ACCOUNTS)

    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(screen.getByText('Network error')).toBeInTheDocument()
    })

    const retryButton = screen.getByRole('button', { name: /Retry/ })
    fireEvent.click(retryButton)

    await waitFor(() => {
      expect(AccountsService.getAccounts).toHaveBeenCalledTimes(2)
    })
  })

  /**
   * Test pagination info display
   */
  it('should display correct pagination info', async () => {
    vi.mocked(TransactionsService.getTransactions).mockResolvedValue({
      transactions: MOCK_TRANSACTIONS,
      total: 25,
      skip: 0,
      take: 10,
    })

    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(screen.getByText('Showing 1 to 10 of 25 transactions')).toBeInTheDocument()
      expect(screen.getByText('Page 1 of 3')).toBeInTheDocument()
    })
  })

  /**
   * Test category filter 'All' option
   */
  it('should show all transactions when All Categories is selected', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(TransactionsService.getTransactions).toHaveBeenCalledWith(
        'acc_001',
        0,
        10,
        undefined
      )
    })

    const categorySelect = screen.getByLabelText('Category') as HTMLSelectElement
    expect(categorySelect.value).toBe('All')
  })

  /**
   * Test color-coded amount display
   */
  it('should color-code amounts based on transaction type', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      const debitAmount = screen.getByText('-$50.00')
      const creditAmount = screen.getByText('+$2,000.00')

      expect(debitAmount).toHaveClass('text-red-400')
      expect(creditAmount).toHaveClass('text-green-400')
    })
  })

  /**
   * Test filters and pagination reset on account change
   */
  it('should reset pagination and filters when account changes', async () => {
    render(
      
        <TransactionsPage />
      
    )

    await waitFor(() => {
      expect(AccountsService.getAccounts).toHaveBeenCalled()
    })

    // Change to category filter
    const categorySelect = screen.getByLabelText('Category') as HTMLSelectElement
    fireEvent.change(categorySelect, { target: { value: 'Food & Dining' } })

    await waitFor(() => {
      expect(TransactionsService.getTransactions).toHaveBeenCalledWith(
        'acc_001',
        0,
        10,
        'Food & Dining'
      )
    })

    vi.clearAllMocks()
    vi.mocked(TransactionsService.getTransactions).mockResolvedValue({
      transactions: MOCK_TRANSACTIONS,
      total: 3,
      skip: 0,
      take: 10,
    })

    // Change account
    const accountSelect = screen.getByLabelText('Account') as HTMLSelectElement
    fireEvent.change(accountSelect, { target: { value: 'acc_002' } })

    await waitFor(() => {
      // Should reset category to 'All' and pagination to page 1
      expect(TransactionsService.getTransactions).toHaveBeenCalledWith(
        'acc_002',
        0,
        10,
        undefined
      )
    })

    // Verify category is reset
    expect(categorySelect.value).toBe('All')
  })

  /**
   * Test disabled state of controls during loading
   */
  it('should disable filter controls while loading', async () => {
    vi.mocked(TransactionsService.getTransactions).mockImplementationOnce(
      () => new Promise(resolve => setTimeout(() => resolve({
        transactions: MOCK_TRANSACTIONS,
        total: 3,
        skip: 0,
        take: 10,
      }), 200))
    )

    render(
      
        <TransactionsPage />
      
    )

    const accountSelect = screen.getByLabelText('Account') as HTMLSelectElement
    const categorySelect = screen.getByLabelText('Category') as HTMLSelectElement
    const pageSizeSelect = screen.getByLabelText('Rows per page') as HTMLSelectElement

    expect(accountSelect).toBeDisabled()
    expect(categorySelect).toBeDisabled()
    expect(pageSizeSelect).toBeDisabled()
  })
})
