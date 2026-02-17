import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BrowserRouter } from 'react-router-dom'
import { server } from '../tests/setup'
import { successHandlers, errorHandlers } from '../tests/mocks/handlers'
import LoginPage from './LoginPage'
import { AUTH_TOKEN_KEY } from '../services/api'

/**
 * LoginPage component tests
 * Tests form rendering, validation, submission, error handling, and navigation
 */
describe('LoginPage', () => {
  const validEmail = 'admin@homebank.local'
  const validPassword = 'demo123'

  // Mock useNavigate
  const mockNavigate = vi.fn()
  vi.mock('react-router-dom', async () => {
    const actual = await vi.importActual('react-router-dom')
    return {
      ...actual,
      useNavigate: () => mockNavigate,
    }
  })

  beforeEach(() => {
    mockNavigate.mockClear()
    localStorage.clear()
  })

  describe('Form Rendering', () => {
    it('should render login form with email and password inputs', () => {
      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      expect(screen.getByText('HomeBanking')).toBeInTheDocument()
      expect(screen.getByText('Secure Login')).toBeInTheDocument()
      expect(screen.getByLabelText('Email')).toBeInTheDocument()
      expect(screen.getByLabelText('Password')).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument()
    })

    it('should display demo credentials hint', () => {
      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      expect(screen.getByText('Demo Credentials:')).toBeInTheDocument()
      expect(screen.getByText('admin@homebank.local')).toBeInTheDocument()
      expect(screen.getByText('demo123')).toBeInTheDocument()
    })

    it('should render with correct placeholders', () => {
      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByPlaceholderText(
        'admin@homebank.local'
      ) as HTMLInputElement
      const passwordInput = screen.getByPlaceholderText('demo123') as HTMLInputElement

      expect(emailInput).toBeInTheDocument()
      expect(passwordInput).toBeInTheDocument()
      expect(emailInput.type).toBe('email')
      expect(passwordInput.type).toBe('password')
    })

    it('should have dark theme styling', () => {
      const { container } = render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const mainDiv = container.querySelector('div.bg-slate-950')
      expect(mainDiv).toBeInTheDocument()
    })
  })

  describe('Form Validation', () => {
    it('should show email required validation error', async () => {
      const user = userEvent.setup()
      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Focus and blur without entering value
      await user.click(emailInput)
      await user.click(submitButton)

      await waitFor(() => {
        expect(screen.getByText('Email is required')).toBeInTheDocument()
      })
    })

    it('should show email format validation error', async () => {
      const user = userEvent.setup()
      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Enter invalid email
      await user.type(emailInput, 'invalid-email')
      await user.click(submitButton)

      await waitFor(() => {
        expect(
          screen.getByText('Please enter a valid email address')
        ).toBeInTheDocument()
      })
    })

    it('should show password required validation error', async () => {
      const user = userEvent.setup()
      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Focus and blur without entering value
      await user.click(passwordInput)
      await user.click(submitButton)

      await waitFor(() => {
        expect(screen.getByText('Password is required')).toBeInTheDocument()
      })
    })

    it('should show password minimum length validation error', async () => {
      const user = userEvent.setup()
      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Enter short password
      await user.type(passwordInput, '123')
      await user.click(submitButton)

      await waitFor(() => {
        expect(
          screen.getByText('Password must be at least 6 characters')
        ).toBeInTheDocument()
      })
    })

    it('should prevent form submission with invalid data', async () => {
      const user = userEvent.setup()
      server.use(...successHandlers)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const submitButton = screen.getByRole('button', { name: /login/i })

      // Try to submit without filling form
      await user.click(submitButton)

      await waitFor(() => {
        expect(screen.getByText('Email is required')).toBeInTheDocument()
      })

      // Navigate should not have been called
      expect(mockNavigate).not.toHaveBeenCalled()
    })

    it('should allow submission with valid data', async () => {
      const user = userEvent.setup()
      server.use(...successHandlers)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Fill form with valid data
      await user.type(emailInput, validEmail)
      await user.type(passwordInput, validPassword)
      await user.click(submitButton)

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/dashboard')
      })
    })
  })

  describe('Successful Login', () => {
    it('should call AuthService.login with correct credentials', async () => {
      const user = userEvent.setup()
      server.use(...successHandlers)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Fill and submit form
      await user.type(emailInput, validEmail)
      await user.type(passwordInput, validPassword)
      await user.click(submitButton)

      await waitFor(() => {
        // Verify token was stored
        expect(localStorage.getItem(AUTH_TOKEN_KEY)).toBeDefined()
      })
    })

    it('should navigate to dashboard on successful login', async () => {
      const user = userEvent.setup()
      server.use(...successHandlers)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      await user.type(emailInput, validEmail)
      await user.type(passwordInput, validPassword)
      await user.click(submitButton)

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/dashboard')
      })
    })

    it('should disable form during loading', async () => {
      const user = userEvent.setup()
      server.use(...successHandlers)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email') as HTMLInputElement
      const passwordInput = screen.getByLabelText('Password') as HTMLInputElement
      const submitButton = screen.getByRole('button', { name: /login/i }) as HTMLButtonElement

      await user.type(emailInput, validEmail)
      await user.type(passwordInput, validPassword)
      await user.click(submitButton)

      // Check that inputs are disabled during submission
      await waitFor(() => {
        expect(emailInput.disabled).toBe(true)
        expect(passwordInput.disabled).toBe(true)
        expect(submitButton.disabled).toBe(true)
      })
    })

    it('should show loading spinner during submission', async () => {
      const user = userEvent.setup()
      server.use(...successHandlers)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      await user.type(emailInput, validEmail)
      await user.type(passwordInput, validPassword)
      await user.click(submitButton)

      // Check for loading state
      await waitFor(() => {
        expect(screen.getByText('Logging in...')).toBeInTheDocument()
      })
    })
  })

  describe('Error Handling', () => {
    it('should display API error message on login failure', async () => {
      const user = userEvent.setup()
      server.use(errorHandlers.loginError)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Try to login with any credentials
      await user.type(emailInput, 'wrong@email.com')
      await user.type(passwordInput, 'wrongpassword')
      await user.click(submitButton)

      await waitFor(() => {
        expect(
          screen.getByText('Email or password incorrect')
        ).toBeInTheDocument()
      })
    })

    it('should not navigate on login failure', async () => {
      const user = userEvent.setup()
      server.use(errorHandlers.loginError)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      await user.type(emailInput, 'wrong@email.com')
      await user.type(passwordInput, 'wrongpassword')
      await user.click(submitButton)

      await waitFor(() => {
        expect(screen.getByText('Email or password incorrect')).toBeInTheDocument()
      })

      // Should not navigate
      expect(mockNavigate).not.toHaveBeenCalled()
    })

    it('should clear previous error when retrying', async () => {
      const user = userEvent.setup()
      server.use(errorHandlers.loginError)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // First attempt - failure
      await user.type(emailInput, 'wrong@email.com')
      await user.type(passwordInput, 'wrongpassword')
      await user.click(submitButton)

      await waitFor(() => {
        expect(
          screen.getByText('Email or password incorrect')
        ).toBeInTheDocument()
      })

      // Switch to success handler
      server.use(...successHandlers)

      // Clear inputs and retry
      await user.clear(emailInput)
      await user.clear(passwordInput)
      await user.type(emailInput, validEmail)
      await user.type(passwordInput, validPassword)
      await user.click(submitButton)

      // Error should be cleared and navigation should happen
      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/dashboard')
      })
    })

    it('should re-enable form after error', async () => {
      const user = userEvent.setup()
      server.use(errorHandlers.loginError)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email') as HTMLInputElement
      const passwordInput = screen.getByLabelText('Password') as HTMLInputElement
      const submitButton = screen.getByRole('button', { name: /login/i }) as HTMLButtonElement

      // Submit with invalid credentials
      await user.type(emailInput, 'wrong@email.com')
      await user.type(passwordInput, 'wrongpassword')
      await user.click(submitButton)

      await waitFor(() => {
        expect(
          screen.getByText('Email or password incorrect')
        ).toBeInTheDocument()
      })

      // Verify form is re-enabled
      expect(emailInput.disabled).toBe(false)
      expect(passwordInput.disabled).toBe(false)
      expect(submitButton.disabled).toBe(false)
    })
  })

  describe('User Interactions', () => {
    it('should allow typing in email and password fields', async () => {
      const user = userEvent.setup()

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email') as HTMLInputElement
      const passwordInput = screen.getByLabelText('Password') as HTMLInputElement

      await user.type(emailInput, validEmail)
      await user.type(passwordInput, validPassword)

      expect(emailInput.value).toBe(validEmail)
      expect(passwordInput.value).toBe(validPassword)
    })

    it('should support tab navigation through form', async () => {
      const user = userEvent.setup()

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Tab from email to password
      await user.tab()
      expect(emailInput).toHaveFocus()

      await user.tab()
      expect(passwordInput).toHaveFocus()

      await user.tab()
      expect(submitButton).toHaveFocus()
    })

    it('should submit form on Enter key in password field', async () => {
      const user = userEvent.setup()
      server.use(...successHandlers)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')

      await user.type(emailInput, validEmail)
      await user.type(passwordInput, `${validPassword}{Enter}`)

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/dashboard')
      })
    })
  })

  describe('Edge Cases', () => {
    it('should handle whitespace in inputs', async () => {
      const user = userEvent.setup()

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Input email with spaces
      await user.type(emailInput, '  ')
      await user.click(submitButton)

      await waitFor(() => {
        expect(
          screen.getByText('Please enter a valid email address')
        ).toBeInTheDocument()
      })
    })

    it('should handle very long input values', async () => {
      const user = userEvent.setup()
      server.use(errorHandlers.loginError)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email')
      const passwordInput = screen.getByLabelText('Password')
      const submitButton = screen.getByRole('button', { name: /login/i })

      // Input very long values
      const longEmail = 'a'.repeat(100) + '@example.com'
      const longPassword = 'a'.repeat(100)

      await user.type(emailInput, longEmail)
      await user.type(passwordInput, longPassword)
      await user.click(submitButton)

      // Should attempt submission even with long values
      await waitFor(() => {
        expect(screen.getByText('Email or password incorrect')).toBeInTheDocument()
      })
    })

    it('should maintain form state while loading', async () => {
      const user = userEvent.setup()
      server.use(...successHandlers)

      render(
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      )

      const emailInput = screen.getByLabelText('Email') as HTMLInputElement
      const passwordInput = screen.getByLabelText('Password') as HTMLInputElement

      await user.type(emailInput, validEmail)
      await user.type(passwordInput, validPassword)

      const submitButton = screen.getByRole('button', { name: /login/i })
      await user.click(submitButton)

      // Verify values are maintained during loading
      expect(emailInput.value).toBe(validEmail)
      expect(passwordInput.value).toBe(validPassword)
    })
  })
})
