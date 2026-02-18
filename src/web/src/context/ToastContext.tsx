import { createContext, useContext, useState, useCallback, type ReactNode } from 'react'

/**
 * Toast types: success, error, info
 */
export type ToastType = 'success' | 'error' | 'info'

/**
 * Individual toast object stored in context
 */
export interface Toast {
  id: string
  message: string
  type: ToastType
  duration: number
}

/**
 * Toast Context type
 */
interface ToastContextType {
  toasts: Toast[]
  addToast: (message: string, type: ToastType, duration?: number) => string
  removeToast: (id: string) => void
  clearAll: () => void
}

/**
 * Create Toast Context
 */
const ToastContext = createContext<ToastContextType | undefined>(undefined)

/**
 * Toast Provider Component
 * Manages toast state and provides add/remove methods
 * - Maximum 3 toasts on screen at once (FIFO)
 * - Auto-dismiss after 5 seconds (default, configurable)
 * - Each toast has unique ID for manual dismissal
 */
interface ToastProviderProps {
  children: ReactNode
}

export function ToastProvider({ children }: ToastProviderProps) {
  const [toasts, setToasts] = useState<Toast[]>([])
  const MAX_TOASTS = 3
  const DEFAULT_DURATION = 5000 // 5 seconds

  /**
   * Add a new toast to the stack
   * If max toasts reached, remove oldest (FIFO)
   * Returns the toast ID for manual dismissal
   */
  const addToast = useCallback(
    (message: string, type: ToastType, duration: number = DEFAULT_DURATION): string => {
      const id = `toast-${Date.now()}-${Math.random()}`

      const newToast: Toast = {
        id,
        message,
        type,
        duration,
      }

      setToasts((prevToasts) => {
        let updated = [...prevToasts, newToast]

        // If max toasts reached, remove oldest (FIFO)
        if (updated.length > MAX_TOASTS) {
          updated = updated.slice(1) // Remove first element
        }

        return updated
      })

      // Auto-dismiss after duration
      setTimeout(() => {
        removeToast(id)
      }, duration)

      return id
    },
    []
  )

  /**
   * Remove a specific toast by ID
   */
  const removeToast = useCallback((id: string) => {
    setToasts((prevToasts) => prevToasts.filter((toast) => toast.id !== id))
  }, [])

  /**
   * Clear all toasts
   */
  const clearAll = useCallback(() => {
    setToasts([])
  }, [])

  const value: ToastContextType = {
    toasts,
    addToast,
    removeToast,
    clearAll,
  }

  return <ToastContext.Provider value={value}>{children}</ToastContext.Provider>
}

/**
 * Hook to access toast context
 * Must be used inside ToastProvider
 */
export function useToastContext(): ToastContextType {
  const context = useContext(ToastContext)
  if (context === undefined) {
    throw new Error('useToastContext must be used inside ToastProvider')
  }
  return context
}
