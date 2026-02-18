# API Services Quick Reference - T012

## Quick Start

```typescript
// Import services
import authService from './services/authService'
import accountsService from './services/accountsService'
import transactionsService from './services/transactionsService'
import transfersService from './services/transfersService'

// Import types
import type { AccountDto, TransactionDto, UserDto } from './types'
```

## AuthService

```typescript
// Login
const response = await authService.login('admin@homebank.local', 'demo123')
// response: { token: string, expiresIn: number, user: UserDto }

// Check authentication
if (authService.isAuthenticated()) {
  // User is logged in
}

// Get current user from JWT
const user = authService.getCurrentUser() // UserDto | null

// Logout
authService.logout()

// Get token
const token = authService.getToken() // string | null
```

## AccountsService

```typescript
// Get all accounts
const accounts = await accountsService.getAccounts()
// accounts: AccountDto[]

// Get specific account
const account = await accountsService.getAccount('acc-001')
// account: AccountDto
```

## TransactionsService

```typescript
// Basic query
const result = await transactionsService.getTransactions('acc-001')
// result: { transactions: TransactionDto[], total: number, skip: 0, take: 10 }

// With pagination
const page2 = await transactionsService.getTransactions('acc-001', 10, 10)

// With category filter
const foodTransactions = await transactionsService.getTransactions(
  'acc-001',
  0,
  10,
  'Food & Dining'
)

// All parameters
const filtered = await transactionsService.getTransactions(
  accountId,
  skip,     // page offset
  take,     // page size
  category  // optional filter
)
```

## TransfersService

```typescript
// Create transfer
const result = await transfersService.createTransfer(
  'acc-001',           // from
  'acc-002',           // to
  500,                 // amount
  'Payment'            // description (optional)
)
// result: { transferId, status, fromAccount, toAccount, amount }
```

## Types Reference

### User
```typescript
interface UserDto {
  id: string
  email: string
}
```

### Account
```typescript
interface AccountDto {
  id: string
  name: string
  balance: number
  currency: string
  lastUpdated: string
}
```

### Transaction
```typescript
interface TransactionDto {
  id: string
  accountId: string
  amount: number
  type: 'Debit' | 'Credit'
  date: string
  description: string
  category: 'Groceries' | 'Utilities' | 'Entertainment' | 'Food & Dining' | 'Transport' | 'Salary' | 'Transfer' | 'Other'
}
```

### Error
```typescript
interface ErrorResponse {
  code: string
  message: string
  details?: Record<string, unknown>
  timestamp: string
}

interface ApiError extends Error {
  response?: {
    status: number
    data: ErrorResponse
  }
}
```

## Error Handling

```typescript
try {
  await accountsService.getAccounts()
} catch (error) {
  // Error has response property with status and data
  if (error instanceof Error && 'response' in error) {
    const apiError = error as any
    console.log(apiError.response.status)     // HTTP status
    console.log(apiError.response.data.code)  // Error code
    console.log(apiError.response.data.message) // Error message
  }
}
```

## JWT Token Management

**Storage**:
- Key: `homebank_jwt`
- Location: `localStorage`
- Automatic injection: Yes (via axios request interceptor)

**Interceptors**:
- Request: Adds `Authorization: Bearer {token}` header
- Response: On 401 error → clears token, redirects to /login

**JWT Decoding**:
```typescript
// Automatic via getCurrentUser()
const user = authService.getCurrentUser()

// JWT format: header.payload.signature
// Payload claims: sub (userId), email, iat (issued at), exp (expiry)
```

## Environment Configuration

```bash
# .env.local or .env.example
VITE_API_URL=http://localhost:5000
```

Accessed in code:
```typescript
const apiUrl = import.meta.env.VITE_API_URL // "http://localhost:5000"
```

## API Base URL

- Development: `http://localhost:5000` (default)
- Override: Set `VITE_API_URL` env variable

**Example for Docker**:
```bash
VITE_API_URL=http://api:5000  # When API runs in Docker container
```

## Common Patterns

### Fetching accounts on component mount
```typescript
useEffect(() => {
  accountsService.getAccounts()
    .then(setAccounts)
    .catch(error => {
      console.error('Failed to load accounts:', error)
      // Show error toast
    })
}, [])
```

### Protected data fetching after login
```typescript
try {
  await authService.login(email, password)
  // JWT automatically stored, will be sent in next requests

  const accounts = await accountsService.getAccounts()
  // Authorization header added automatically
} catch (error) {
  // Handle login error
}
```

### Handling 401 errors
```typescript
// Automatic: Any 401 response triggers:
// 1. localStorage token cleared
// 2. Redirect to /login
// 3. Error thrown for component to handle

// In component:
try {
  await accountsService.getAccounts()
} catch (error) {
  // This component will be unmounted as router redirects to /login
}
```

### Pagination pattern
```typescript
const pageSize = 10
const page = 0

const result = await transactionsService.getTransactions(
  accountId,
  page * pageSize,  // skip
  pageSize          // take
)

// result.total = total number of transactions
// result.transactions = current page data
// result.skip = offset used
// result.take = page size used
```

### Filtering transactions
```typescript
const categories = ['Food & Dining', 'Utilities', 'Transport']

for (const category of categories) {
  const result = await transactionsService.getTransactions(
    accountId,
    0,
    100,
    category
  )
  console.log(`${category}: ${result.transactions.length} transactions`)
}
```

## Testing

All services are tested with MSW (Mock Service Worker):

```bash
cd src/web
npm run test -- --run        # Run all tests once
npm run test                 # Run in watch mode (requires Node 18+)
npm run test -- --coverage   # With coverage report
```

**Test files**:
- `src/services/authService.test.ts`
- `src/services/accountsService.test.ts`
- `src/services/transactionsService.test.ts`
- `src/services/transfersService.test.ts`

---

**Documentation**: See `/IMPLEMENTATION_SUMMARY_T012.md` for full details
