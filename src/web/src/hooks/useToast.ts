import { useToastContext } from '../context/ToastContext'

/**
 * useToast Hook
 *
 * Provides convenience methods for showing toast notifications:
 * - .success(message, duration?) - Green success toast
 * - .error(message, duration?) - Red error toast
 * - .info(message, duration?) - Blue info toast
 *
 * Each method returns the toast ID for manual dismissal.
 *
 * Example usage:
 *   const toast = useToast()
 *   toast.success('Transfer completed!')
 *   toast.error('Insufficient balance')
 *   toast.info('Loading...')
 */
export function useToast() {
  const { addToast } = useToastContext()

  return {
    /**
     * Show success toast (green)
     * @param message - Toast message
     * @param duration - Auto-dismiss duration in ms (default: 5000)
     * @returns Toast ID for manual dismissal
     */
    success: (message: string, duration?: number): string => {
      return addToast(message, 'success', duration)
    },

    /**
     * Show error toast (red)
     * @param message - Toast message
     * @param duration - Auto-dismiss duration in ms (default: 5000)
     * @returns Toast ID for manual dismissal
     */
    error: (message: string, duration?: number): string => {
      return addToast(message, 'error', duration)
    },

    /**
     * Show info toast (blue)
     * @param message - Toast message
     * @param duration - Auto-dismiss duration in ms (default: 5000)
     * @returns Toast ID for manual dismissal
     */
    info: (message: string, duration?: number): string => {
      return addToast(message, 'info', duration)
    },
  }
}
