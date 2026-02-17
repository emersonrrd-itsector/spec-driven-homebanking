import { useToastContext } from '../../context/ToastContext'
import { X, CheckCircle, AlertCircle, Info } from 'lucide-react'

/**
 * Toaster Component
 * Renders toast notifications in the top-right corner
 * - Success: Green background, success icon
 * - Error: Red background, alert icon
 * - Info: Blue background, info icon
 * - Each toast has a close button for manual dismissal
 * - Auto-dismisses after configured duration
 * - Maximum 3 toasts on screen at once
 */
export function Toaster() {
  const { toasts, removeToast } = useToastContext()

  if (toasts.length === 0) {
    return null
  }

  return (
    <div className="fixed top-4 right-4 z-50 space-y-2">
      {toasts.map((toast) => {
        const isSuccess = toast.type === 'success'
        const isError = toast.type === 'error'

        const bgColor = isSuccess
          ? 'bg-green-600'
          : isError
            ? 'bg-red-600'
            : 'bg-blue-600'

        const icon = isSuccess ? (
          <CheckCircle className="h-5 w-5 flex-shrink-0" />
        ) : isError ? (
          <AlertCircle className="h-5 w-5 flex-shrink-0" />
        ) : (
          <Info className="h-5 w-5 flex-shrink-0" />
        )

        return (
          <div
            key={toast.id}
            className={`${bgColor} text-white rounded-lg shadow-lg p-4 flex items-start gap-3 max-w-sm animate-in fade-in slide-in-from-right-4 duration-300`}
            role="alert"
          >
            {icon}
            <div className="flex-1">
              <p className="text-sm font-medium">{toast.message}</p>
            </div>
            <button
              onClick={() => removeToast(toast.id)}
              className="text-white hover:opacity-80 transition-opacity flex-shrink-0"
              aria-label="Close toast"
            >
              <X className="h-4 w-4" />
            </button>
          </div>
        )
      })}
    </div>
  )
}
