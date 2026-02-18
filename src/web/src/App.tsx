import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { ToastProvider } from './context/ToastContext'
import { Toaster } from './components/ui/toaster'
import Layout from './components/Layout/Layout'
import ProtectedRoute from './components/Layout/ProtectedRoute'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import TransactionsPage from './pages/TransactionsPage'
import TransferPage from './pages/TransferPage'
import AuthService from './services/authService'

/**
 * App: Main application component with React Router configuration
 * Routes:
 *   - /login: Public login page
 *   - /dashboard: Protected dashboard (default for authenticated users)
 *   - /transactions: Protected transactions page
 *   - /transfer: Protected transfer page
 *   - /: Root redirects to /dashboard if authenticated, /login otherwise
 *
 * Wrapped with ToastProvider for global toast notification support
 */
function App() {
  return (
    <ToastProvider>
      <Router>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />

          {/* Root redirects based on auth state */}
          <Route
            path="/"
            element={
              AuthService.isAuthenticated() ? (
                <Navigate to="/dashboard" replace />
              ) : (
                <Navigate to="/login" replace />
              )
            }
          />

          {/* Protected routes with layout */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Layout>
                  <DashboardPage />
                </Layout>
              </ProtectedRoute>
            }
          />

          <Route
            path="/transactions"
            element={
              <ProtectedRoute>
                <Layout>
                  <TransactionsPage />
                </Layout>
              </ProtectedRoute>
            }
          />

          <Route
            path="/transfer"
            element={
              <ProtectedRoute>
                <Layout>
                  <TransferPage />
                </Layout>
              </ProtectedRoute>
            }
          />

          {/* Catch-all: redirect to root */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Router>

      {/* Global Toaster component - renders toasts in top-right corner */}
      <Toaster />
    </ToastProvider>
  )
}

export default App
