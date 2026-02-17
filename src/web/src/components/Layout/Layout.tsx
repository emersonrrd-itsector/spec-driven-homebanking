import type { ReactNode } from 'react'
import Header from './Header'
import Sidebar from './Sidebar'

interface LayoutProps {
  children: ReactNode
}

/**
 * Layout: Main application layout with Header + Sidebar + content area
 * Only used for authenticated/protected routes (not on /login)
 * Applies dark theme styling consistently
 */
function Layout({ children }: LayoutProps) {
  return (
    <div className="min-h-screen bg-slate-950 text-slate-50">
      <Header />
      <div className="flex">
        <Sidebar />
        <main className="flex-1 p-6 md:p-8">
          <div className="container mx-auto">{children}</div>
        </main>
      </div>
    </div>
  )
}

export default Layout
