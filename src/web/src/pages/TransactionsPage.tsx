/**
 * TransactionsPage: Transactions list and filtering placeholder
 * Full implementation with transaction table coming in T016
 */
function TransactionsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-4xl font-bold text-white mb-2">Transactions</h1>
        <p className="text-slate-400">View and filter your transactions</p>
      </div>

      <div className="bg-slate-900 rounded-lg border border-slate-700 p-8 text-center">
        <p className="text-slate-400 mb-4">Transactions Content</p>
        <p className="text-sm text-slate-500">
          Transaction table and filters coming in T016
        </p>
      </div>
    </div>
  )
}

export default TransactionsPage
