# Home Banking Application — Specification

**Status**: MVP v1.0-alpha  
**Last Updated**: 2026-02-17  
**Author**: Spec-Driven Development Team

---

## 1. Functional Requirements

### 1.1 User Management
- **FR-1.1**: User can authenticate with email and password
  - Demo account: `admin@homebank.local` / `demo123`
  - JWT token issued on valid credentials, stored in `localStorage`
  - Token includes `userId` and `email` claims
  - Token expiry: 24 hours
  
- **FR-1.2**: User remains authenticated across page reloads (token persistence)

### 1.2 Account Management
- **FR-2.1**: User can view all their accounts
  - Shows: account name, balance (formatted with currency), last updated timestamp
  - Displayed as interactive cards on dashboard
  
- **FR-2.2**: User can view account details
  - GET `/api/accounts/{id}` returns: id, name, balance, currency, accountNumber, lastUpdated

### 1.3 Transaction History
- **FR-3.1**: User can view transactions for a selected account
  - Paginated (default 10 per page)
  - Filterable by category (optional multi-select)
  - Columns: date, description, category (badge), amount (formatted), type (debit/credit)
  
- **FR-3.2**: Categories are predefined
  - Groceries, Utilities, Entertainment, Food & Dining, Transport, Salary, Transfer, Other
  - Each category has a color for badge styling

### 1.4 Transfers
- **FR-4.1**: User can initiate a transfer between their own accounts
  - Form inputs: from account (dropdown), to account (dropdown), amount (number), description (optional)
  - Validation: amount > $0.01, source ≠ destination, sufficient balance
  
- **FR-4.2**: On successful transfer
  - New transaction created in source account (debit, "Transfer to [account]")
  - New transaction created in destination account (credit, "Transfer from [account]")
  - Balances immediately updated
  - Success toast notification shown
  
- **FR-4.3**: On failed transfer
  - Error message: InsufficientBalance, AccountNotFound, InvalidTransfer, etc.
  - User can retry

### 1.5 Dashboard
- **FR-5.1**: Dashboard displays:
  - Welcome message with logged-in user name
  - Account cards grid (responsive layout)
  - Quick navigation to Transactions view
  - Quick navigation to Transfer form

---

## 2. Non-Functional Requirements

### 2.1 Performance
- **NFR-1.1**: Dashboard load time < 2s (API + render)
- **NFR-1.2**: Transaction list pagination response < 500ms
- **NFR-1.3**: Transfer submission response < 1s

### 2.2 Accessibility
- **NFR-2.1**: All interactive elements keyboard-navigable (Tab, Enter)
- **NFR-2.2**: Form labels associated with inputs via `<label htmlFor>`
- **NFR-2.3**: Color not sole means of information (category badges include text + color)
- **NFR-2.4**: Dark theme has sufficient contrast (WCAG AA minimum)

### 2.3 Security
- **NFR-3.1**: All protected endpoints require JWT token
- **NFR-3.2**: Tokens validated on every API request
- **NFR-3.3**: Password stored as SHA-256 hash (demo only; production use bcrypt)
- **NFR-3.4**: CORS enabled for frontend origin only
- **NFR-3.5**: No sensitive data in localStorage except JWT token
- **NFR-3.6**: JWT secret stored as environment variable (not in code)

### 2.4 Reliability
- **NFR-4.1**: All API endpoints have health checks and logging
- **NFR-4.2**: Errors always return structured response: `{ code, message, details, timestamp }`
- **NFR-4.3**: No unhandled exceptions in API (all caught globally)
- **NFR-4.4**: Failed API calls show user-friendly error messages

### 2.5 Maintainability
- **NFR-5.1**: Unit test coverage: API >80%, Frontend >70%
- **NFR-5.2**: E2E test coverage: 3 critical flows (login, view dashboard, transfer)
- **NFR-5.3**: No TypeScript errors or warnings
- **NFR-5.4**: No ESLint warnings in frontend
- **NFR-5.5**: No StyleCop warnings in API code

### 2.6 Scalability (Future Considerations)
- **NFR-6.1**: Architecture supports migration from InMemory to SQL Server / PostgreSQL
  - Repository pattern used for data access
  - DbContext abstraction ready for production DB swap
- **NFR-6.2**: Docker containerization enables cloud deployment (Azure App Service, AKS, etc.)

---

## 3. Architecture Overview

### 3.1 Folder Structure

```
spec-driven-homebanking/
├── src/
│   ├── api/                                 # .NET Core 9 Web API
│   │   ├── HomeBanking.API.csproj
│   │   ├── Program.cs                       # DI, middleware setup
│   │   ├── appsettings.json
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs            # POST /auth (login)
│   │   │   ├── AccountsController.cs        # GET /accounts, /accounts/{id}
│   │   │   ├── TransactionsController.cs    # GET /transactions
│   │   │   ├── TransfersController.cs       # POST /transfers
│   │   │   └── HealthController.cs          # GET /health, /health/ready
│   │   ├── Services/
│   │   │   ├── IAuthService.cs
│   │   │   ├── AuthService.cs               # JWT token generation
│   │   │   ├── ITransferService.cs
│   │   │   ├── TransferService.cs           # Transfer validation & execution
│   │   │   └── IAccountService.cs
│   │   ├── Models/
│   │   │   ├── User.cs
│   │   │   ├── Account.cs
│   │   │   ├── Transaction.cs
│   │   │   └── (other domain entities)
│   │   ├── DTOs/
│   │   │   ├── Requests/
│   │   │   └── Responses/
│   │   ├── Data/
│   │   │   ├── HomeBankingContext.cs        # EF Core DbContext
│   │   │   └── SeedData.cs
│   │   ├── Middleware/
│   │   │   ├── AuthenticationMiddleware.cs
│   │   │   └── GlobalExceptionHandlerMiddleware.cs
│   │   └── Logging/
│   │       └── (structured logging setup)
│   │
│   └── web/                                 # React + Vite + TypeScript
│       ├── package.json
│       ├── vite.config.ts
│       ├── tsconfig.json
│       ├── tailwind.config.ts
│       ├── postcss.config.ts
│       ├── vitest.config.ts
│       ├── playwright.config.ts
│       ├── src/
│       │   ├── main.tsx
│       │   ├── App.tsx
│       │   ├── pages/
│       │   │   ├── LoginPage.tsx
│       │   │   ├── DashboardPage.tsx
│       │   │   ├── TransactionsPage.tsx
│       │   │   └── TransferPage.tsx
│       │   ├── components/
│       │   │   ├── Layout/
│       │   │   │   ├── Header.tsx
│       │   │   │   ├── Sidebar.tsx
│       │   │   │   └── ProtectedRoute.tsx
│       │   │   ├── AccountCard.tsx
│       │   │   ├── TransactionTable.tsx
│       │   │   ├── TransferForm.tsx
│       │   │   └── (other components)
│       │   ├── services/
│       │   │   ├── api.ts                  # axios instance
│       │   │   ├── authService.ts
│       │   │   ├── accountService.ts
│       │   │   ├── transactionService.ts
│       │   │   └── transferService.ts
│       │   ├── hooks/
│       │   │   └── useToast.ts
│       │   ├── types/
│       │   │   └── index.ts                # TypeScript interfaces for DTOs
│       │   ├── context/
│       │   │   └── ToastContext.tsx
│       │   └── styles/
│       │       └── (Tailwind + shadcn/ui theming)
│       ├── tests/
│       │   ├── **/*.test.tsx
│       │   └── setup.ts
│       └── e2e/
│           ├── dashboard.spec.ts
│           └── transfer.spec.ts
│
├── tests/                                   # API Integration Tests (xUnit)
│   ├── HomeBanking.Tests.csproj
│   ├── Controllers/
│   ├── Services/
│   └── Fixtures/
│
├── .github/
│   └── workflows/
│       └── ci.yml                           # GitHub Actions CI/CD
│
├── Dockerfile.api                           # Multi-stage, .NET 9
├── Dockerfile.web                           # Node 22 + Nginx
├── docker-compose.yml                       # Local dev orchestration
├── .gitignore
├── spec.md                                  # This file
├── plan.md                                  # Task breakdown
└── README.md                                # Getting started
```

### 3.2 API Architecture

**Layering** (Clean Architecture):

```
Presentation Layer (Controllers)
        ↓
Application/Service Layer (Services, Business Logic)
        ↓
Data Access Layer (Repository, DbContext)
        ↓
Database (InMemory or SQL Server)
```

**Key Components:**

1. **Controllers** (HomeBanking.API)
   - Handle HTTP requests/responses
   - Validate input models
   - Call services for business logic
   - Return appropriate status codes

2. **Services** (Business Logic)
   - `AuthService`: JWT token generation, credential validation
   - `TransferService`: Transfer validation, transaction recording
   - `AccountService`: Account queries, balance calculations

3. **Data Access** (EF Core)
   - `HomeBankingContext`: DbContext with navigations
   - In-memory provider (v1.0)
   - Seed data on startup

4. **DTOs** (API Contracts)
   - Request: `LoginRequest`, `TransferRequest`
   - Response: `AccountDto`, `TransactionDto`, `ErrorResponse`

### 3.3 Frontend Architecture

**Routing** (React Router v6):

```
/login                          → LoginPage (unauthenticated)
/dashboard                      → DashboardPage (protected)
/transactions                   → TransactionsPage (protected)
/transfer                       → TransferPage (protected)
(default: /dashboard if token exists, /login if not)
```

**State Management**:

- **Auth**: Stored in `localStorage` (JWT token)
- **UI State**: React hooks (`useState`, `useContext`)
- **Toast Notifications**: Context-based (avoid global state complexity)
- **API Calls**: Service layer with error handling

**Component Hierarchy**:

```
<App>
  ├─ <ProtectedRoute>
  │   ├─ <Layout>
  │   │   ├─ <Header> (logout, user name)
  │   │   ├─ <Sidebar> (nav links)
  │   │   └─ <Page>
  │   │       ├─ <DashboardPage>
  │   │       │   └─ <AccountCard> (grid)
  │   │       ├─ <TransactionsPage>
  │   │       │   └─ <TransactionTable> (table + filters)
  │   │       └─ <TransferPage>
  │   │           └─ <TransferForm>
  └─ <LoginPage> (public)
```

**Dark Theme**: Tailwind class-based (`dark:` prefix), shadcn/ui `use-dark-mode` hook

---

## 4. Tech Stack with Version Pins

### 4.1 Backend (.NET Core 9)

| Component | Version | Rationale |
|-----------|---------|-----------|
| .NET SDK | 9.0.102 | Latest LTS, container-optimized |
| C# | 13 | Latest with nullable reference types |
| EF Core | 9.0.2 | Latest, excellent InMemory support |
| Scalar | 1.2.x | OpenAPI UI better than Swagger |
| xUnit | 2.8.1 | Standard testing framework |

**Dependencies** (NuGet):
```
Microsoft.EntityFrameworkCore.InMemory 9.0.2
Microsoft.AspNetCore.Authentication.JwtBearer 9.0.1
System.IdentityModel.Tokens.Jwt 7.3.0
Serilog 4.0.1
Serilog.AspNetCore 9.0.0
```

### 4.2 Frontend (React + Vite)

| Component | Version | Rationale |
|-----------|---------|-----------|
| Node.js | 22.0.0 LTS | Latest LTS, stable |
| React | 19.0.0 | Latest, improved hooks |
| TypeScript | 5.7.x | Strict mode enabled |
| Vite | 6.1.x | Fastest bundler, HMR |
| SWC | Latest | Fast transpiler via vite plugin |
| Tailwind CSS | 3.4.x | Dark mode support |
| shadcn/ui | Latest | Headless component library |
| React Router | 6.28.x | Standard routing library |
| Axios | 1.7.x | Promise-based HTTP client |
| Vitest | 2.0.x | Fast unit test runner |
| React Testing Library | 16.0.x | Component testing best practice |
| Playwright | 1.48.x | E2E testing, fast & reliable |

**DevDependencies**:
```
ESLint 9.x, Prettier 4.x, @typescript-eslint/parser 8.x
Tailwind CSS 3.4.x, PostCSS 8.x
```

### 4.3 Infrastructure

| Component | Version | Purpose |
|-----------|---------|---------|
| Docker | Latest | Containerization |
| Docker Compose | 2.20.x | Local orchestration |
| GitHub Actions | (latest) | CI/CD |

---

## 5. Key Design Decisions & Rationale

### 5.1 Why InMemory Database (Now) → SQL Server (Later)?

**Decision**: Start with EF Core InMemory, designed for migration to SQL Server/PostgreSQL.

**Rationale**:
- ✅ **Fast MVP**: No database setup, no migrations, focus on features
- ✅ **Learning**: Understand domain logic without DB complexity
- ✅ **CI/CD**: Faster tests (no DB provisioning in GitHub Actions)
- ✅ **Migration Path**: Repository pattern + DbContext abstraction make migration trivial
- ⚠️ **Limitation**: InMemory data cleared on app restart (demo only)

**Migration Path** (post-v1.0):
1. Add `Microsoft.EntityFrameworkCore.SqlServer` NuGet
2. Update `Program.cs`: `.AddDbContext<HomeBankingContext>(options => options.UseSqlServer(...))`
3. Create migrations: `dotnet ef migrations add InitialCreate`
4. Update GitHub Actions to provision test database

### 5.2 Why JWT with Hardcoded Demo Accounts?

**Decision**: No OAuth2/Azure AD; use JWT with demo credentials.

**Rationale**:
- ✅ **Simplicity**: Focus on core banking features, not auth infrastructure
- ✅ **Learning**: JWT token flow is educational
- ✅ **Speed**: No external service dependency
- ✅ **Testing**: E2E tests don't need real accounts
- ⚠️ **Scope**: Demo only; production would use Azure AD or OAuth2

**Demo Credentials**:
```
username: admin@homebank.local
password: demo123
```

### 5.3 Why React + Vite + TypeScript?

**Decision**: Modern, type-safe frontend.

**Rationale**:
- ✅ **TypeScript**: Catches errors at compile time (DTOs, API responses)
- ✅ **Vite**: Fastest HMR, fastest build (vs Create React App)
- ✅ **React 19**: Latest features, better performance
- ✅ **Tailwind + shadcn/ui**: Rapid UI development, dark theme built-in

### 5.4 Why Tailwind CSS + shadcn/ui?

**Decision**: Component-based styling with dark theme support.

**Rationale**:
- ✅ **Utility-first**: Consistent spacing, colors, typography
- ✅ **Dark mode**: `dark:` class prefix handles theme switching
- ✅ **shadcn/ui**: Pre-built, accessible components (Button, Card, Form, Table)
- ✅ **No CSS-in-JS**: Faster than styled-components, cleaner HTML
- ✅ **Customizable**: Easy to override Tailwind defaults

### 5.5 Why Playwright for E2E Tests?

**Decision**: Playwright for critical user flows.

**Rationale**:
- ✅ **Multi-browser**: Run on Chromium, Firefox, WebKit
- ✅ **Parallel execution**: Tests run fast
- ✅ **Debugging**: Screenshots, videos on failure
- ✅ **Reliability**: No flaky tests (good waits)
- ✅ **Docker**: Runs in CI/CD without extra setup

**E2E Flows** (3 critical scenarios):
1. **Login**: Navigate to /login, enter credentials, receive JWT
2. **Dashboard**: View accounts, transactions, balances
3. **Transfer**: Fill form, submit, see success/error

### 5.6 Why Docker from Day 1?

**Decision**: Containerized API (Dockerfile.api) and Web (Dockerfile.web) + Docker Compose.

**Rationale**:
- ✅ **Consistency**: "Works on my machine" → "Works everywhere"
- ✅ **CI/CD Ready**: GitHub Actions can test in containers
- ✅ **Production Parity**: Local dev mirrors production environment
- ✅ **Easy onboarding**: New devs run `docker compose up` (no SDK installation required)

### 5.7 Why GitHub Actions for CI?

**Decision**: GitHub-native CI/CD.

**Rationale**:
- ✅ **Integration**: Tight with GitHub repo (no external service)
- ✅ **Free**: 2,000 free minutes/month
- ✅ **Simple YAML**: Easy to read and maintain
- ✅ **Matrix builds**: Can test multiple .NET versions, Node versions

**CI Pipeline**:
1. Trigger: PR created/updated
2. Build .NET API + unit tests
3. Build React frontend + unit tests
4. Lint (ESLint, StyleCop)
5. E2E tests (Playwright)
6. All pass → ready to merge

---

## 6. API Contract Examples

### 6.1 Authentication

**POST /auth**

Request:
```json
{
  "email": "admin@homebank.local",
  "password": "demo123"
}
```

Response (200):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 86400,
  "user": {
    "id": "usr_123",
    "email": "admin@homebank.local"
  }
}
```

Error (401):
```json
{
  "code": "InvalidCredentials",
  "message": "Email or password incorrect",
  "details": null,
  "timestamp": "2026-02-17T14:30:00Z"
}
```

### 6.2 Accounts

**GET /api/accounts**

Response (200):
```json
{
  "accounts": [
    {
      "id": "acc_001",
      "name": "Checking",
      "balance": 5000.00,
      "currency": "USD",
      "lastUpdated": "2026-02-17T12:00:00Z"
    }
  ]
}
```

### 6.3 Transactions

**GET /api/transactions?accountId=acc_001&skip=0&take=10**

Response (200):
```json
{
  "transactions": [
    {
      "id": "txn_001",
      "accountId": "acc_001",
      "amount": 50.00,
      "type": "debit",
      "date": "2026-02-16T10:30:00Z",
      "description": "Coffee Shop",
      "category": "Food & Dining"
    }
  ],
  "total": 42,
  "skip": 0,
  "take": 10
}
```

### 6.4 Transfers

**POST /api/transfers**

Request:
```json
{
  "fromAccountId": "acc_001",
  "toAccountId": "acc_002",
  "amount": 500.00,
  "description": "Payment to savings"
}
```

Response (201):
```json
{
  "transferId": "xfr_001",
  "status": "completed",
  "fromAccount": { "id": "acc_001", "newBalance": 4500.00 },
  "toAccount": { "id": "acc_002", "newBalance": 1500.00 },
  "amount": 500.00
}
```

Error (400 — Insufficient Balance):
```json
{
  "code": "InsufficientBalance",
  "message": "Transfer amount exceeds available balance",
  "details": {
    "available": 100.00,
    "requested": 500.00
  },
  "timestamp": "2026-02-17T14:30:00Z"
}
```

---

## 7. Demo Data Seeding

**Users**:
```
User (id: usr_001)
├─ Email: admin@homebank.local
└─ Password: demo123 (hashed)
```

**Accounts**:
```
Account 1 (Checking, $5,000)
Account 2 (Savings, $10,000)
Account 3 (Business, $25,000)
```

**Transactions** (per account):
- 10-15 sample transactions
- Mix of categories: Groceries, Utilities, Food, Transport, Salary, Other
- Dates: spread over last 30 days
- Both debit and credit types

**Example**:
```
2026-02-16: Salary Deposit, +$2,000, Salary
2026-02-14: Coffee Shop, -$5.50, Food & Dining
2026-02-12: Electric Bill, -$120, Utilities
2026-02-10: Transfer to Savings, -$500, Transfer
```

---

## 8. Future Considerations (Post-MVP)

1. **Database Migration**: InMemory → SQL Server / PostgreSQL
2. **OAuth2/Azure AD**: Replace hardcoded credentials
3. **Payment Processing**: Stripe integration
4. **Mobile App**: React Native or Flutter
5. **Notifications**: Email/SMS on transfers
6. **Analytics**: Transaction trends, spending insights
7. **Budget Management**: Set spending limits per category
8. **Multi-currency**: Support EUR, GBP, JPY, etc.
9. **API Rate Limiting**: Prevent abuse
10. **Advanced Security**: 2FA, device fingerprinting

---

**Next**: See `plan.md` for atomic task breakdown and implementation roadmap.
