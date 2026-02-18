import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { Loader2 } from 'lucide-react'
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
import { useToast } from '../hooks/useToast'
import AuthService from '../services/authService'
import type { ApiError } from '../types'

/**
 * LoginPage: Full-featured login form with validation and API integration
 * - Email + password form with client-side validation
 * - shadcn/ui Form component for layout and error display
 * - AuthService integration for JWT token storage
 * - Loading state during API request
 * - Error handling with user-friendly messages
 */
function LoginPage() {
  const navigate = useNavigate()
  const toast = useToast()
  const [isLoading, setIsLoading] = useState(false)
  const [apiError, setApiError] = useState<string | null>(null)

  // Initialize form with validation rules
  const form = useForm({
    defaultValues: {
      email: '',
      password: '',
    },
    mode: 'onSubmit', // Validate on submit for better test compatibility
  })

  /**
   * Handle form submission
   * Calls AuthService.login() and navigates to dashboard on success
   */
  const handleSubmit = async (data: { email: string; password: string }) => {
    setApiError(null)
    setIsLoading(true)

    try {
      // Call AuthService which stores JWT in localStorage
      await AuthService.login(data.email, data.password)

      // Show success toast
      toast.success('Logged in successfully!')

      // Navigate to dashboard on success
      navigate('/dashboard')
    } catch (error) {
      // Handle API errors and network errors
      const apiErr = error as ApiError
      let errorMessage = 'Login failed. Please try again.'

      if (apiErr.response?.data?.message) {
        // API returned structured error response
        errorMessage = apiErr.response.data.message
      } else if (apiErr.message) {
        // Network or other error
        errorMessage = apiErr.message
      }

      setApiError(errorMessage)
      toast.error(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-950 p-4">
      <div className="w-full max-w-md">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-white mb-2">HomeBanking</h1>
          <p className="text-slate-400">Secure Login</p>
        </div>

        {/* Login Form Container */}
        <div className="bg-slate-900 rounded-lg border border-slate-700 p-8">
          {/* Demo Credentials Hint */}
          <div className="mb-6 p-3 bg-slate-800 border border-slate-700 rounded text-sm text-slate-300">
            <p className="font-semibold text-slate-200 mb-1">Demo Credentials:</p>
            <p>Email: <span className="font-mono text-amber-300">admin@homebank.local</span></p>
            <p>Password: <span className="font-mono text-amber-300">demo123</span></p>
          </div>

          {/* API Error Message */}
          {apiError && (
            <div className="mb-6 p-3 bg-red-900/20 border border-red-700/30 rounded text-sm text-red-300">
              {apiError}
            </div>
          )}

          {/* Form */}
          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(handleSubmit)}
              className="space-y-5"
              noValidate
            >
              {/* Email Field */}
              <FormField
                control={form.control}
                name="email"
                rules={{
                  required: 'Email is required',
                  pattern: {
                    value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                    message: 'Please enter a valid email address',
                  },
                }}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-slate-200">Email</FormLabel>
                    <FormControl>
                      <Input
                        {...field}
                        type="email"
                        placeholder="admin@homebank.local"
                        disabled={isLoading}
                        className="bg-slate-800 border-slate-700 text-white placeholder:text-slate-500"
                        autoComplete="email"
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Password Field */}
              <FormField
                control={form.control}
                name="password"
                rules={{
                  required: 'Password is required',
                  minLength: {
                    value: 6,
                    message: 'Password must be at least 6 characters',
                  },
                }}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-slate-200">Password</FormLabel>
                    <FormControl>
                      <Input
                        {...field}
                        type="password"
                        placeholder="demo123"
                        disabled={isLoading}
                        className="bg-slate-800 border-slate-700 text-white placeholder:text-slate-500"
                        autoComplete="current-password"
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Submit Button */}
              <Button
                type="submit"
                disabled={isLoading}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white"
              >
                {isLoading ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Logging in...
                  </>
                ) : (
                  'Login'
                )}
              </Button>
            </form>
          </Form>
        </div>

        {/* Footer */}
        <p className="text-center text-xs text-slate-400 mt-6">
          This is a demo application. Use the credentials above to log in.
        </p>
      </div>
    </div>
  )
}

export default LoginPage
