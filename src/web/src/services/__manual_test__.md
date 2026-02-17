# Manual Test Instructions for API Services

## Service Implementation Verification

### 1. AuthService
**Location**: `src/services/authService.ts`

Methods implemented:
- `login(email, password)` - Posts to `/api/auth`, stores JWT in localStorage
- `logout()` - Clears JWT token
- `isAuthenticated()` - Checks if token exists
- `getToken()` - Returns stored JWT
- `getCurrentUser()` - Decodes JWT to extract user info

Test credentials: `admin@homebank.local` / `demo123`

### 2. AccountsService
**Location**: `src/services/accountsService.ts`

Methods implemented:
- `getAccounts()` - Fetches all accounts from `/api/accounts`
- `getAccount(id)` - Fetches specific account from `/api/accounts/{id}`

### 3. TransactionsService
**Location**: `src/services/transactionsService.ts`

Methods implemented:
- `getTransactions(accountId, skip?, take?, category?)` - Fetches paginated transactions from `/api/transactions` with optional category filter

Parameters:
- `accountId`: string (required)
- `skip`: number (default 0)
- `take`: number (default 10)
- `category`: TransactionCategory (optional)

### 4. TransfersService
**Location**: `src/services/transfersService.ts`

Methods implemented:
- `createTransfer(fromAccountId, toAccountId, amount, description?)` - Posts transfer request to `/api/transfers`

### 5. API Configuration
**Location**: `src/services/api.ts`

Features:
- Axios instance configured with baseURL from `VITE_API_URL` env (default: http://localhost:5000)
- Request interceptor adds JWT token to Authorization header
- Response interceptor handles 401 errors (redirects to /login, clears token)
- Environment configuration via `.env.local` or `.env.example`

### 6. TypeScript Types
**Location**: `src/types/index.ts`

DTOs implemented:
- `LoginRequest`, `LoginResponse`, `UserDto`
- `AccountDto`, `AccountsResponse`
- `TransactionDto`, `TransactionsResponse`, `TransactionFilter`, `TransactionType`, `TransactionCategory`
- `TransferRequest`, `TransferResponse`, `TransferAccountResponse`
- `ErrorResponse`, `ApiError`

## Testing Strategy

### Unit Tests with MSW (Mock Service Worker)

Test files location: `src/services/*.test.ts`

**MSW Configuration**:
- File: `src/tests/mocks/handlers.ts` - Contains all API mocks
- Setup: `src/tests/setup.ts` - Initializes MSW server, localStorage mock, crypto polyfill

**Test Coverage**:

1. **authService.test.ts** (6 test suites)
   - Login with valid/invalid credentials
   - Logout clears token
   - isAuthenticated checks
   - getCurrentUser decoding
   - Token persistence

2. **accountsService.test.ts** (2 test suites)
   - Get all accounts
   - Get specific account
   - 404 error handling
   - Server error handling

3. **transactionsService.test.ts** (1 test suite with 10 test cases)
   - Fetch transactions with pagination
   - Category filtering
   - Date ordering (descending)
   - Valid transaction types and categories
   - Total count accuracy

4. **transfersService.test.ts** (1 test suite with 10 test cases)
   - Create transfer with valid data
   - Balance updates
   - Insufficient balance error
   - Invalid transfer error (same account)
   - Decimal amount handling
   - Amount validation

### Integration Points

**Environment Variables** (`.env.local`):
```
VITE_API_URL=http://localhost:5000
```

**localStorage Keys**:
- `homebank_jwt` - Stores JWT token

**API Endpoints Used**:
- POST `/api/auth` - Login
- GET `/api/accounts` - List accounts
- GET `/api/accounts/{id}` - Get account detail
- GET `/api/transactions` - List transactions
- POST `/api/transfers` - Create transfer

## Running Tests Locally

### Prerequisites
- Node.js >= 18 (Vitest 2+ requires Node 18+)
- npm 8+

### Steps
1. `cd src/web`
2. `npm install` (already done)
3. `npm run build` ✓ (verified working)
4. `npm run lint` ✓ (verified working)
5. `npm run test -- --run` (requires Node 18+)

### Current Status
- **Build**: ✓ PASSING (0 errors)
- **Lint**: ✓ PASSING (0 errors)
- **TypeScript**: ✓ PASSING (0 errors with `tsc --noEmit`)
- **Tests**: Ready to run (requires Node.js upgrade to 18+)

## Notes

The service layer is fully implemented and typed with comprehensive tests using MSW mocking. The tests are designed to:
- Mock all API endpoints without network calls
- Test happy paths and error scenarios
- Validate request/response structure and types
- Test pagination, filtering, and error handling
- Ensure proper localStorage usage for JWT tokens
- Validate JWT decoding and user extraction

All code follows TypeScript strict mode requirements with proper type-only imports and is compatible with production builds.
