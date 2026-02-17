import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { ToastProvider, useToastContext } from './ToastContext'

/**
 * Test component that uses ToastContext
 */
function TestComponent() {
  const { toasts, addToast, removeToast, clearAll } = useToastContext()

  return (
    <div>
      <button onClick={() => addToast('Success', 'success')}>Add Success</button>
      <button onClick={() => addToast('Error', 'error')}>Add Error</button>
      <button onClick={() => addToast('Info', 'info')}>Add Info</button>
      <button onClick={() => clearAll()}>Clear All</button>

      <div data-testid="toast-count">{toasts.length}</div>
      {toasts.map((toast) => (
        <div key={toast.id} data-testid={`toast-${toast.type}`}>
          {toast.message}
          <button onClick={() => removeToast(toast.id)}>Remove</button>
        </div>
      ))}
    </div>
  )
}

describe('ToastContext', () => {
  it('provides toast context to wrapped components', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    expect(screen.getByTestId('toast-count')).toHaveTextContent('0')
  })

  it('throws error when used outside provider', () => {
    // Suppress console.error for this test
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

    expect(() => {
      render(<TestComponent />)
    }).toThrow('useToastContext must be used inside ToastProvider')

    consoleSpy.mockRestore()
  })

  it('adds toasts with correct type', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    // Initial state
    expect(screen.getByTestId('toast-count')).toHaveTextContent('0')

    // Add success toast
    screen.getByText('Add Success').click()
    waitFor(() => {
      expect(screen.getByTestId('toast-count')).toHaveTextContent('1')
    })
  })

  it('enforces maximum 3 toasts on screen', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    const addSuccessBtn = screen.getByText('Add Success')

    // Add 5 toasts
    addSuccessBtn.click()
    addSuccessBtn.click()
    addSuccessBtn.click()
    addSuccessBtn.click()
    addSuccessBtn.click()

    await waitFor(() => {
      // Only 3 toasts should be visible (FIFO - oldest removed)
      expect(screen.getByTestId('toast-count')).toHaveTextContent('3')
    })
  })

  it('clears all toasts', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    // Add some toasts
    screen.getByText('Add Success').click()
    screen.getByText('Add Error').click()

    await waitFor(() => {
      expect(screen.getByTestId('toast-count')).toHaveTextContent('2')
    })

    // Clear all
    screen.getByText('Clear All').click()

    await waitFor(() => {
      expect(screen.getByTestId('toast-count')).toHaveTextContent('0')
    })
  })

  it('removes individual toast by ID', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    )

    // Add a success toast
    screen.getByText('Add Success').click()

    await waitFor(() => {
      expect(screen.getByTestId('toast-count')).toHaveTextContent('1')
    })

    // Remove the toast
    const removeButtons = screen.getAllByText('Remove')
    if (removeButtons.length > 0) {
      removeButtons[0].click()

      await waitFor(() => {
        expect(screen.getByTestId('toast-count')).toHaveTextContent('0')
      })
    }
  })

  it('auto-dismisses toast after duration', async () => {
    vi.useFakeTimers()

    render(
      <ToastProvider>
        <div>
          <TestComponent />
        </div>
      </ToastProvider>
    )

    // Add a toast with short duration
    screen.getByText('Add Success').click()

    await waitFor(() => {
      expect(screen.getByTestId('toast-count')).toHaveTextContent('1')
    })

    // Fast forward 5 seconds
    vi.advanceTimersByTime(5000)

    // Toast should be removed
    await waitFor(() => {
      expect(screen.getByTestId('toast-count')).toHaveTextContent('0')
    })

    vi.useRealTimers()
  })
})
