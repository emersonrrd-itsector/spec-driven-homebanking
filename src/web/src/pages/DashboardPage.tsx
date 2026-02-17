/**
 * DashboardPage: Dashboard placeholder showing account overview
 * Full implementation with account cards coming in T015
 */
function DashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-4xl font-bold text-white mb-2">Dashboard</h1>
        <p className="text-slate-400">Welcome to your accounts overview</p>
      </div>

      <div className="bg-slate-900 rounded-lg border border-slate-700 p-8 text-center">
        <p className="text-slate-400 mb-4">Dashboard Content</p>
        <p className="text-sm text-slate-500">
          Account cards and overview coming in T015
        </p>
      </div>
    </div>
  )
}

export default DashboardPage
