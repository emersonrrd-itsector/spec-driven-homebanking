import { Link, useLocation } from 'react-router-dom'
import { LayoutDashboard, ArrowRightLeft, History } from 'lucide-react'

/**
 * Sidebar: Navigation menu with links to main app sections
 * Shows active link highlighting for current route
 * Responsive: collapses on mobile (placeholder for future mobile enhancement)
 */
function Sidebar() {
  const location = useLocation()

  const navItems = [
    { path: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
    { path: '/transactions', label: 'Transactions', icon: History },
    { path: '/transfer', label: 'Transfer', icon: ArrowRightLeft },
  ]

  const isActive = (path: string) => location.pathname === path

  return (
    <aside className="hidden md:block w-64 border-r border-slate-700 bg-slate-900 p-6">
      <nav className="space-y-2">
        {navItems.map(({ path, label, icon: Icon }) => (
          <Link
            key={path}
            to={path}
            className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
              isActive(path)
                ? 'bg-slate-800 text-white font-semibold'
                : 'text-slate-400 hover:text-white hover:bg-slate-800'
            }`}
          >
            <Icon size={20} />
            <span>{label}</span>
          </Link>
        ))}
      </nav>
    </aside>
  )
}

export default Sidebar
