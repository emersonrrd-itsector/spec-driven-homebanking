import { useNavigate } from 'react-router-dom'
import { Button } from '../ui/button'
import AuthService from '../../services/authService'

/**
 * Header: Application header with logo, user info, and logout button
 * Displays on protected routes (not on login page)
 */
function Header() {
  const navigate = useNavigate()
  const user = AuthService.getCurrentUser()

  const handleLogout = () => {
    AuthService.logout()
    navigate('/login', { replace: true })
  }

  return (
    <header className="border-b border-slate-700 bg-slate-950 p-4">
      <div className="container mx-auto flex items-center justify-between">
        <div className="flex items-center">
          <h1 className="text-2xl font-bold text-white">HomeBanking</h1>
        </div>

        <div className="flex items-center gap-6">
          {user && (
            <>
              <div className="text-right">
                <p className="text-sm text-slate-400">Logged in as</p>
                <p className="text-sm font-medium text-white">{user.email}</p>
              </div>
              <Button
                variant="outline"
                onClick={handleLogout}
                className="bg-slate-900 text-white hover:bg-slate-800 hover:text-white border-slate-700"
              >
                Logout
              </Button>
            </>
          )}
        </div>
      </div>
    </header>
  )
}

export default Header
