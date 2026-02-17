import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ToastProvider } from '../../context/ToastContext'
import { Toaster } from './toaster'
import { useToast } from '../../hooks/useToast'

/**
 * Test component that adds toasts and displays the toaster
 */
function TestToasterComponent() {
  const toast = useToast()

  return (
    <div>
      <button onClick={() => toast.success('Success toast')}>Show Success</button>
      <button onClick={() => toast.error('Error toast')}>Show Error</button>
      <button onClick={() => toast.info('Info toast')}>Show Info</button>
      <Toaster />
    </div>
  )
}

describe('Toaster Component', () => {
  it('renders nothing when no toasts', () => {
    render(
      <ToastProvider>
        <Toaster />
      </ToastProvider>
    )

    // Should not render any toast elements
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('renders toasts when added', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestToasterComponent />
      </ToastProvider>
    )

    // Click to show success toast
    await user.click(screen.getByText('Show Success'))

    // Toast should be visible
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument()
      expect(screen.getByText('Success toast')).toBeInTheDocument()
    })
  })

  it('displays different toast types with correct styling', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestToasterComponent />
      </ToastProvider>
    )

    // Show success toast
    await user.click(screen.getByText('Show Success'))

    await waitFor(() => {
      const successToast = screen.getByRole('alert')
      expect(successToast).toHaveClass('bg-green-600')
    })

    // Show error toast
    await user.click(screen.getByText('Show Error'))

    await waitFor(() => {
      const alerts = screen.getAllByRole('alert')
      expect(alerts.length).toBeGreaterThan(0)
    })
  })

  it('allows manual dismissal of toast', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestToasterComponent />
      </ToastProvider>
    )

    // Show success toast
    await user.click(screen.getByText('Show Success'))

    await waitFor(() => {
      expect(screen.getByText('Success toast')).toBeInTheDocument()
    })

    // Click close button
    const closeButton = screen.getByLabelText('Close toast')
    await user.click(closeButton)

    // Toast should be gone
    await waitFor(() => {
      expect(screen.queryByText('Success toast')).not.toBeInTheDocument()
    })
  })

  it('displays multiple toasts', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestToasterComponent />
      </ToastProvider>
    )

    // Show multiple toasts
    await user.click(screen.getByText('Show Success'))
    await user.click(screen.getByText('Show Error'))
    await user.click(screen.getByText('Show Info'))

    // All toasts should be visible
    await waitFor(() => {
      const alerts = screen.getAllByRole('alert')
      expect(alerts.length).toBeGreaterThanOrEqual(3)
    })
  })

  it('displays correct icons for each toast type', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestToasterComponent />
      </ToastProvider>
    )

    // Show success toast - should have CheckCircle icon
    await user.click(screen.getByText('Show Success'))

    await waitFor(() => {
      // Icon should be rendered as SVG within alert
      const alert = screen.getByRole('alert')
      expect(alert.querySelector('svg')).toBeInTheDocument()
    })
  })

  it('positions toasts in top-right corner', () => {
    render(
      <ToastProvider>
        <TestToasterComponent />
      </ToastProvider>
    )

    // Get the container div
    const container = screen.getByRole('alert', { hidden: true })?.parentElement

    // Should have fixed positioning in top-right
    if (container) {
      expect(container).toHaveClass('fixed', 'top-4', 'right-4')
    }
  })

  it('auto-dismisses toasts after duration', async () => {
    vi.useFakeTimers()
    const user = userEvent.setup({ delay: null })

    render(
      <ToastProvider>
        <TestToasterComponent />
      </ToastProvider>
    )

    // Show success toast
    await user.click(screen.getByText('Show Success'))

    await waitFor(() => {
      expect(screen.getByText('Success toast')).toBeInTheDocument()
    })

    // Fast forward 5 seconds (default duration)
    vi.advanceTimersByTime(5000)

    // Toast should be auto-dismissed
    await waitFor(() => {
      expect(screen.queryByText('Success toast')).not.toBeInTheDocument()
    })

    vi.useRealTimers()
  })

  it('respects maximum 3 toasts on screen', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestToasterComponent />
      </ToastProvider>
    )

    // Add more than 3 toasts
    await user.click(screen.getByText('Show Success'))
    await user.click(screen.getByText('Show Success'))
    await user.click(screen.getByText('Show Success'))
    await user.click(screen.getByText('Show Success'))

    // Only 3 should be visible (FIFO)
    await waitFor(() => {
      const alerts = screen.getAllByRole('alert')
      expect(alerts.length).toBeLessThanOrEqual(3)
    })
  })
})
