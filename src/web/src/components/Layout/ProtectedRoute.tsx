import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import AuthService from '../../services/authService'

interface ProtectedRouteProps {
  children: ReactNode
}

/**
 * ProtectedRoute: Wrapper component that checks JWT token authentication
 * Redirects unauthenticated users to /login with return URL for post-login navigation
 */
function ProtectedRoute({ children }: ProtectedRouteProps) {
  const location = useLocation()
  const isAuthenticated = AuthService.isAuthenticated()

  if (!isAuthenticated) {
    // Redirect to login, preserving the location they were trying to access
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return <>{children}</>
}

export default ProtectedRoute
