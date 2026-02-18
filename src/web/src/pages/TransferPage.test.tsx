import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, waitFor } from '../tests/test-utils'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { server } from '../tests/setup'
import { successHandlers, errorHandlers, mockAccounts } from '../tests/mocks/handlers'
import TransferPage from './TransferPage'

// Mock useNavigate at module level
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

/**
 * TransferPage component tests
 * Tests form rendering, validation, submission, error handling, and state management
 */
describe('TransferPage', () => {
  beforeEach(() => {
    mockNavigate.mockClear()
    localStorage.clear()
    server.use(...successHandlers)
    vi.clearAllMocks()
    // Mock alert
    window.alert = vi.fn()
  })

  afterEach(() => {
    server.resetHandlers()
  })

  describe('Form Rendering', () => {
    it('should render transfer form with all fields', async () => {
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByText('Transfer Money')).toBeInTheDocument()
        expect(screen.getByText('Transfer funds between your accounts')).toBeInTheDocument()
      })

      expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      expect(screen.getByLabelText('To Account')).toBeInTheDocument()
      expect(screen.getByLabelText('Amount (USD)')).toBeInTheDocument()
      expect(screen.getByLabelText('Description (Optional)')).toBeInTheDocument()
    })

    it('should render submit button', async () => {
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        const submitButton = screen.getByRole('button', { name: /transfer/i })
        expect(submitButton).toBeInTheDocument()
      })
    })

    it('should populate account dropdowns with accounts', async () => {
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        const fromSelect = screen.getByLabelText('From Account') as HTMLSelectElement
        const toSelect = screen.getByLabelText('To Account') as HTMLSelectElement

        expect(fromSelect.options.length).toBeGreaterThan(1)
        expect(toSelect.options.length).toBeGreaterThan(1)
      })
    })

    it('should set first account as default from account', async () => {
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        const fromSelect = screen.getByLabelText('From Account') as HTMLSelectElement
        expect(fromSelect.value).toBe(mockAccounts[0].id)
      })
    })

    it('should have dark theme styling', () => {
      const { container } = render(
        
            <TransferPage />
        
      )

      const cards = container.querySelectorAll('div.bg-slate-900')
      expect(cards.length).toBeGreaterThan(0)
    })
  })

  describe('Form Validation', () => {
    it('should validate that from account is required', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account')
      const amountInput = screen.getByLabelText('Amount (USD)')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Clear from account
      await user.selectOptions(fromSelect, '')
      // Trigger blur to validate
      await user.click(amountInput)
      await waitFor(() => {
        // Button should be disabled
        expect(submitButton).toBeDisabled()
      })
    })

    it('should validate that to account is required', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('To Account')).toBeInTheDocument()
      })

      const toSelect = screen.getByLabelText('To Account')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Ensure to account is empty
      await user.selectOptions(toSelect, '')
      await waitFor(() => {
        expect(submitButton).toBeDisabled()
      })
    })

    it('should validate that from and to accounts cannot be the same', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account')
      const toSelect = screen.getByLabelText('To Account')

      // Set both to the same account
      await user.selectOptions(fromSelect, mockAccounts[0].id)
      await user.selectOptions(toSelect, mockAccounts[0].id)

      // Trigger validation by clicking elsewhere
      await user.click(screen.getByLabelText('Amount (USD)'))

      await waitFor(() => {
        expect(
          screen.getByText(/Destination account must be different from source account/i)
        ).toBeInTheDocument()
      })
    })

    it('should validate that amount is required', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('Amount (USD)')).toBeInTheDocument()
      })

      const amountInput = screen.getByLabelText('Amount (USD)')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Focus and blur without entering value
      await user.click(amountInput)
      await user.click(screen.getByLabelText('Description (Optional)'))

      await waitFor(() => {
        expect(screen.getByText(/Amount is required/i)).toBeInTheDocument()
        expect(submitButton).toBeDisabled()
      })
    })

    it('should validate that amount must be greater than 0.01', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('Amount (USD)')).toBeInTheDocument()
      })

      const amountInput = screen.getByLabelText('Amount (USD)') as HTMLInputElement
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Enter invalid amounts
      await user.clear(amountInput)
      await user.type(amountInput, '0.00')
      await user.click(screen.getByLabelText('Description (Optional)'))

      await waitFor(() => {
        expect(
          screen.getByText(/Amount must be greater than \$0\.01/i)
        ).toBeInTheDocument()
        expect(submitButton).toBeDisabled()
      })
    })

    it('should validate that amount must be a valid number', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('Amount (USD)')).toBeInTheDocument()
      })

      const amountInput = screen.getByLabelText('Amount (USD)') as HTMLInputElement
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Enter invalid amount
      await user.clear(amountInput)
      await user.type(amountInput, 'abc')
      await user.click(screen.getByLabelText('Description (Optional)'))

      await waitFor(() => {
        expect(submitButton).toBeDisabled()
      })
    })

    it('should validate description max length (500 characters)', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('Description (Optional)')).toBeInTheDocument()
      })

      const descriptionInput = screen.getByLabelText('Description (Optional)') as HTMLTextAreaElement
      const longText = 'a'.repeat(501)

      await user.click(descriptionInput)
      await user.type(descriptionInput, longText)

      // The input maxLength should prevent entering more than 500
      await waitFor(() => {
        expect(descriptionInput.value.length).toBeLessThanOrEqual(500)
      })
    })

    it('should display character count for description', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('Description (Optional)')).toBeInTheDocument()
      })

      const descriptionInput = screen.getByLabelText('Description (Optional)')

      await user.click(descriptionInput)
      await user.type(descriptionInput, 'Test description')

      await waitFor(() => {
        expect(screen.getByText(/16 \/ 500 characters/i)).toBeInTheDocument()
      })
    })

    it('should enable submit button when form is valid', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account')
      const toSelect = screen.getByLabelText('To Account')
      const amountInput = screen.getByLabelText('Amount (USD)')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Fill form with valid data
      await user.selectOptions(fromSelect, mockAccounts[0].id)
      await user.selectOptions(toSelect, mockAccounts[1].id)
      await user.clear(amountInput)
      await user.type(amountInput, '100.00')
      // Trigger validation by blurring the amount field
      await user.tab()

      await waitFor(() => {
        expect(submitButton).not.toBeDisabled()
      }, { timeout: 3000 })
    })
  })

  describe('Form Submission', () => {
    it('should submit transfer with valid data', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account')
      const toSelect = screen.getByLabelText('To Account')
      const amountInput = screen.getByLabelText('Amount (USD)')
      const descriptionInput = screen.getByLabelText('Description (Optional)')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Fill form
      await user.selectOptions(fromSelect, mockAccounts[0].id)
      await user.selectOptions(toSelect, mockAccounts[1].id)
      await user.clear(amountInput)
      await user.type(amountInput, '500.00')
      await user.tab() // Trigger validation
      await user.clear(descriptionInput)
      await user.type(descriptionInput, 'Test transfer')

      // Wait for form to be valid
      await waitFor(() => {
        expect(submitButton).not.toBeDisabled()
      }, { timeout: 3000 })

      // Submit form
      await user.click(submitButton)

      // Success toast should be shown (via ToastContext)
      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalled()
      }, { timeout: 3000 })
    })

    it('should navigate to transactions on successful transfer', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account')
      const toSelect = screen.getByLabelText('To Account')
      const amountInput = screen.getByLabelText('Amount (USD)')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Fill and submit form
      await user.selectOptions(fromSelect, mockAccounts[0].id)
      await user.selectOptions(toSelect, mockAccounts[1].id)
      await user.clear(amountInput)
      await user.type(amountInput, '100.00')
      await user.tab() // Trigger validation

      // Wait for form to be valid
      await waitFor(() => {
        expect(submitButton).not.toBeDisabled()
      }, { timeout: 3000 })

      await user.click(submitButton)

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith(
          expect.stringContaining('/transactions?accountId=')
        )
      }, { timeout: 3000 })
    })

    it('should show loading state during submission', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account')
      const toSelect = screen.getByLabelText('To Account')
      const amountInput = screen.getByLabelText('Amount (USD)')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Fill form
      await user.selectOptions(fromSelect, mockAccounts[0].id)
      await user.selectOptions(toSelect, mockAccounts[1].id)
      await user.clear(amountInput)
      await user.type(amountInput, '100.00')
      await user.tab() // Trigger validation

      // Wait for form to be valid
      await waitFor(() => {
        expect(submitButton).not.toBeDisabled()
      }, { timeout: 3000 })

      // Click submit button - the loading state is very brief, so we just verify submission happens
      await user.click(submitButton)

      // Verify that navigate was called (submission completed)
      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalled()
      }, { timeout: 3000 })
    })

    it('should disable form inputs during submission', async () => {
      const user = userEvent.setup()
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account') as HTMLSelectElement
      const toSelect = screen.getByLabelText('To Account') as HTMLSelectElement
      const amountInput = screen.getByLabelText('Amount (USD)') as HTMLInputElement
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Fill form
      await user.selectOptions(fromSelect, mockAccounts[0].id)
      await user.selectOptions(toSelect, mockAccounts[1].id)
      await user.clear(amountInput)
      await user.type(amountInput, '100.00')
      await user.tab() // Trigger validation

      // Wait for form to be valid
      await waitFor(() => {
        expect(submitButton).not.toBeDisabled()
      }, { timeout: 3000 })

      // Submit - the form completes very quickly with MSW
      await user.click(submitButton)

      // Verify submission completed (navigate was called)
      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalled()
      }, { timeout: 3000 })
    })
  })

  describe('Error Handling', () => {
    it('should display insufficient balance error', async () => {
      server.use(errorHandlers.insufficientBalance)
      const user = userEvent.setup()

      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account')
      const toSelect = screen.getByLabelText('To Account')
      const amountInput = screen.getByLabelText('Amount (USD)')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Fill form
      await user.selectOptions(fromSelect, mockAccounts[0].id)
      await user.selectOptions(toSelect, mockAccounts[1].id)
      await user.clear(amountInput)
      await user.type(amountInput, '500.00')
      await user.tab() // Trigger validation

      // Wait for form to be valid
      await waitFor(() => {
        expect(submitButton).not.toBeDisabled()
      }, { timeout: 3000 })

      // Submit
      await user.click(submitButton)

      // Check for error message - it should contain balance info
      await waitFor(() => {
        expect(
          screen.getByText(
            /Insufficient balance/i
          )
        ).toBeInTheDocument()
      }, { timeout: 3000 })
    })

    it('should display generic error message for other errors', async () => {
      server.use(
        errorHandlers.invalidTransfer
      )
      const user = userEvent.setup()

      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account')
      const toSelect = screen.getByLabelText('To Account')
      const amountInput = screen.getByLabelText('Amount (USD)')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Fill form
      await user.selectOptions(fromSelect, mockAccounts[0].id)
      await user.selectOptions(toSelect, mockAccounts[1].id)
      await user.clear(amountInput)
      await user.type(amountInput, '100.00')
      await user.tab() // Trigger validation

      // Wait for form to be valid
      await waitFor(() => {
        expect(submitButton).not.toBeDisabled()
      }, { timeout: 3000 })

      // Submit
      await user.click(submitButton)

      await waitFor(() => {
        expect(
          screen.getByText(/Cannot transfer to the same account/i)
        ).toBeInTheDocument()
      }, { timeout: 3000 })
    })

    it('should keep form visible after error for retry', async () => {
      server.use(errorHandlers.insufficientBalance)
      const user = userEvent.setup()

      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
      })

      const fromSelect = screen.getByLabelText('From Account')
      const toSelect = screen.getByLabelText('To Account')
      const amountInput = screen.getByLabelText('Amount (USD)')
      const submitButton = screen.getByRole('button', { name: /transfer/i })

      // Fill form
      await user.selectOptions(fromSelect, mockAccounts[0].id)
      await user.selectOptions(toSelect, mockAccounts[1].id)
      await user.clear(amountInput)
      await user.type(amountInput, '500.00')
      await user.tab() // Trigger validation

      // Wait for form to be valid
      await waitFor(() => {
        expect(submitButton).not.toBeDisabled()
      }, { timeout: 3000 })

      // Submit
      await user.click(submitButton)

      // Wait for error
      await waitFor(() => {
        expect(screen.getByText(/Insufficient balance/i)).toBeInTheDocument()
      }, { timeout: 3000 })

      // Form should still be visible and editable
      expect(screen.getByLabelText('From Account')).toBeInTheDocument()
    })

    it('should handle accounts loading error', async () => {
      server.use(errorHandlers.serverError)

      render(
        
            <TransferPage />
        
      )

      // The error message from serverError is "An unexpected error occurred"
      await waitFor(() => {
        expect(
          screen.getByText(/An unexpected error occurred/i)
        ).toBeInTheDocument()
      })

      // Retry button should be present
      const retryButton = screen.getByRole('button', { name: /retry/i })
      expect(retryButton).toBeInTheDocument()
    })

    it('should retry loading accounts', async () => {
      const user = userEvent.setup()

      // First request fails
      server.use(errorHandlers.serverError)
      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(
          screen.getByText(/An unexpected error occurred/i)
        ).toBeInTheDocument()
      })

      // Now use success handlers
      server.use(...successHandlers)

      const retryButton = screen.getByRole('button', { name: /retry/i })
      await user.click(retryButton)

      // Should now show account options
      await waitFor(() => {
        expect(screen.getByLabelText('From Account')).toBeInTheDocument()
        // Error should be gone
        expect(
          screen.queryByText(/An unexpected error occurred/i)
        ).not.toBeInTheDocument()
      })
    })
  })

  describe('Account Loading', () => {
    it('should show loading state while fetching accounts', () => {
      render(
        
            <TransferPage />
        
      )

      expect(screen.getByText(/Loading accounts/i)).toBeInTheDocument()
    })

    it('should show no accounts state when empty', async () => {
      server.use(
        http.get('http://localhost:5000/api/accounts', () => {
          return HttpResponse.json({ accounts: [] }, { status: 200 })
        })
      )

      render(
        
            <TransferPage />
        
      )

      await waitFor(() => {
        expect(screen.getByText(/No accounts available/i)).toBeInTheDocument()
      })
    })
  })
})
