# Home Banking: Task Plan

**Status**: Ready for execution  
**Start Date**: 2026-02-17  
**Target Completion**: End of Week 3 (2026-03-02)  

---

## Overview

25 atomic tasks organized in 5 phases. Each task is independently deployable and testable. Dependencies are clear; tasks can be parallelized where possible.

**Phases**:
1. **API Foundation** (T001-T010): .NET scaffolding, data layer, REST endpoints, auth
2. **Frontend Foundation** (T011-T015): React setup, layout, login, dashboard
3. **Transaction & Transfer UI** (T016-T018): Tables, forms, notifications
4. **Testing** (T019-T022): Unit tests (API & frontend), E2E tests
5. **Infrastructure & CI/CD** (T023-T025): Docker, GitHub Actions, documentation

---

## Task Breakdown

### Phase 1: API Foundation (T001-T010)

#### Task T001: .NET 9 Project Scaffolding
- **Status**: pending
- **Dependencies**: none
- **Estimate**: S
- **DoD**: 
  - [ ] Solution with 3 projects: API, Domain, Tests scaffolded
  - [ ] Global usings configured
  - [ ] Directory.Build.props with common settings
  - [ ] builds cleanly (`dotnet build`)
  - [ ] no lint warnings (StyleCop)
  - [ ] committed

#### Task T002: EF Core InMemory Context Setup
- **Status**: pending
- **Dependencies**: T001
- **Estimate**: M
- **DoD**:
  - [ ] DbContext created with InMemory provider
  - [ ] Models: User, Account, Transaction defined
  - [ ] Seed data initialization (2-3 accounts, ~15 transactions per account)
  - [ ] DbContext tests validate seeding
  - [ ] all tests pass
  - [ ] committed

#### Task T003: Core Domain Models & Value Objects
- **Status**: pending
- **Dependencies**: T002
- **Estimate**: M
- **DoD**:
  - [ ] User aggregate (Id, Email, PasswordHash)
  - [ ] Account aggregate (Id, UserId, Balance, Currency, AccountNumber)
  - [ ] Transaction DTO (Id, AccountId, Amount, Date, Description, Category, Type)
  - [ ] Validation rules defined (balance ≥ 0, transfer amount > 0, etc.)
  - [ ] unit tests pass
  - [ ] committed

#### Task T004: JWT Authentication Middleware
- **Status**: pending
- **Dependencies**: T003
- **Estimate**: M
- **DoD**:
  - [ ] JwtTokenService generates tokens (hardcoded demo credentials: admin@homebank.local/demo123)
  - [ ] AuthenticationMiddleware validates JWT on protected endpoints
  - [ ] Demo user seeding (admin@homebank.local, password hashed)
  - [ ] POST /auth endpoint returns JWT on valid credentials
  - [ ] 401 response on invalid credentials
  - [ ] unit tests for token generation/validation/expiry
  - [ ] all tests pass
  - [ ] committed

#### Task T005: Account Controller - Get Accounts & Balances
- **Status**: pending
- **Dependencies**: T004
- **Estimate**: M
- **DoD**:
  - [ ] GET /api/accounts returns user's accounts with balance
  - [ ] GET /api/accounts/{id} returns single account detail
  - [ ] Response DTO: { id, name, balance, currency, lastUpdated }
  - [ ] Authorized (JWT required)
  - [ ] Unit tests (200, 401, 404 cases)
  - [ ] Scalar OpenAPI schema generated and readable
  - [ ] all tests pass
  - [ ] committed

#### Task T006: Transaction Controller - Get Transactions
- **Status**: pending
- **Dependencies**: T005
- **Estimate**: M
- **DoD**:
  - [ ] GET /api/transactions?accountId={id} returns paginated transactions
  - [ ] Response DTO: { id, amount, date, description, category, type }
  - [ ] Pagination support (default: skip=0, take=10)
  - [ ] Filtering by category optional
  - [ ] Authorized (JWT required)
  - [ ] Unit tests (200, 401, pagination, filtering)
  - [ ] Scalar OpenAPI schema updated
  - [ ] all tests pass
  - [ ] committed

#### Task T007: Transfer Validation & Business Rules
- **Status**: pending
- **Dependencies**: T006
- **Estimate**: M
- **DoD**:
  - [ ] TransferService validates: sufficient balance, accounts exist, from ≠ to
  - [ ] Error responses: InsufficientBalance, AccountNotFound, InvalidTransfer
  - [ ] Transaction created in source account (debit, "Transfer to [name]")
  - [ ] Transaction created in destination account (credit, "Transfer from [name]")
  - [ ] Balances updated atomically
  - [ ] Unit tests cover all validation scenarios
  - [ ] all tests pass
  - [ ] committed

#### Task T008: Transfer Controller - POST Transfer
- **Status**: pending
- **Dependencies**: T007
- **Estimate**: M
- **DoD**:
  - [ ] POST /api/transfers accepts { fromAccountId, toAccountId, amount, description }
  - [ ] Input validation (amount > 0.01, description optional)
  - [ ] Calls TransferService for business logic
  - [ ] Returns 201 { transferId, status, fromAccount, toAccount, amount }
  - [ ] On error: 400 with error DTO { code, message, details, timestamp }
  - [ ] Authorized (JWT required)
  - [ ] Unit tests (201, 400 bad balance, 404 account not found)
  - [ ] Scalar OpenAPI schema updated
  - [ ] all tests pass
  - [ ] committed

#### Task T009: Health Check Endpoint
- **Status**: pending
- **Dependencies**: T001
- **Estimate**: S
- **DoD**:
  - [ ] GET /health returns { status: "healthy", timestamp, version }
  - [ ] GET /health/ready checks database connectivity
  - [ ] Unauthenticated (public endpoint)
  - [ ] Unit tests (200 response, JSON format)
  - [ ] all tests pass
  - [ ] committed

#### Task T010: API Error Response Standardization
- **Status**: pending
- **Dependencies**: T009
- **Estimate**: S
- **DoD**:
  - [ ] Global exception handling middleware
  - [ ] All errors return { code, message, details, timestamp }
  - [ ] Handles 400 (validation), 401 (auth), 404, 500 scenarios
  - [ ] Errors logged to console/file with request context
  - [ ] Unit tests for error middleware
  - [ ] all tests pass
  - [ ] committed

---

### Phase 2: Frontend Foundation (T011-T015)

#### Task T011: React + Vite + TypeScript Project Setup
- **Status**: pending
- **Dependencies**: none (parallel to T001-T010)
- **Estimate**: M
- **DoD**:
  - [ ] Vite project scaffolded with React 19 + TypeScript 5.7
  - [ ] react-swc-ts configured for fast refresh
  - [ ] Tailwind CSS 3.4 configured with dark mode (class strategy)
  - [ ] shadcn/ui initialized (Button, Card, Input, Table, Form components)
  - [ ] Dark theme as default (dark class on html element)
  - [ ] builds to dist/ cleanly (`npm run build`)
  - [ ] no lint warnings (ESLint + @typescript-eslint)
  - [ ] committed

#### Task T012: API Client Service & Types
- **Status**: pending
- **Dependencies**: T011, T004
- **Estimate**: M
- **DoD**:
  - [ ] axios instance configured for API calls (baseURL from env)
  - [ ] AuthService: login(email, password) → JWT stored in localStorage
  - [ ] AccountsService: getAccounts(), getAccount(id)
  - [ ] TransactionsService: getTransactions(accountId, filters)
  - [ ] TransfersService: createTransfer(payload)
  - [ ] TypeScript interfaces for all DTOs (strict typing)
  - [ ] Request/response interceptors for auth headers and error handling
  - [ ] 401 interceptor redirects to login
  - [ ] Unit tests mock API calls with MSW (Mock Service Worker)
  - [ ] all tests pass
  - [ ] committed

#### Task T013: Layout & Navigation
- **Status**: pending
- **Dependencies**: T012
- **Estimate**: S
- **DoD**:
  - [ ] Main layout component: Header (logo, user name, logout) + Sidebar (nav links)
  - [ ] React Router configured: /, /dashboard, /transactions, /transfer, /login
  - [ ] ProtectedRoute wrapper checks JWT; redirects unauthenticated users to /login
  - [ ] Dark theme applied consistently (Tailwind + shadcn/ui)
  - [ ] Responsive layout (mobile-first Tailwind, sidebar collapses on mobile)
  - [ ] Navigation active state highlights current route
  - [ ] committed

#### Task T014: Login Page
- **Status**: pending
- **Dependencies**: T013
- **Estimate**: S
- **DoD**:
  - [ ] Form: email + password inputs (both required)
  - [ ] Validation: email format check, password ≥ 6 chars (client-side)
  - [ ] shadcn/ui Form component used for layout and error display
  - [ ] Submit button: calls AuthService.login(), disables during request
  - [ ] Success: JWT stored, redirected to /dashboard
  - [ ] Error: displays error message (API response or network error)
  - [ ] Demo credentials shown as hint/placeholder
  - [ ] Dark theme ready (shadcn/ui form styling)
  - [ ] committed

#### Task T015: Dashboard Page - Account Cards
- **Status**: pending
- **Dependencies**: T014, T005
- **Estimate**: M
- **DoD**:
  - [ ] Calls getAccounts() on mount
  - [ ] Renders shadcn/ui Card for each account (responsive grid: 1-3 columns)
  - [ ] Card shows: account name, balance (formatted currency, e.g., $5,000.00), last updated
  - [ ] Card clickable: navigates to /transfers or shows account detail modal
  - [ ] Loading state: skeleton cards (Tailwind pulse effect)
  - [ ] Error state: error message + retry button
  - [ ] Dark theme ready
  - [ ] Unit tests (render, loading, error states)
  - [ ] all tests pass
  - [ ] committed

---

### Phase 3: Transaction & Transfer UI (T016-T018)

#### Task T016: Transactions Page - Table + Filters
- **Status**: pending
- **Dependencies**: T015, T006
- **Estimate**: M
- **DoD**:
  - [ ] Account selector dropdown (populated from accounts list)
  - [ ] Calls getTransactions(accountId) on load and account change
  - [ ] shadcn/ui Table with columns: Date, Description, Category, Amount
  - [ ] Category badges (colored by category: Food=red, Utilities=blue, etc.)
  - [ ] Category filter selector (select from predefined categories)
  - [ ] Pagination: prev/next buttons, page size selector (10/25/50)
  - [ ] Loading state: table skeleton
  - [ ] Empty state: "No transactions" message
  - [ ] Dark theme ready
  - [ ] Unit tests (render, filter, pagination)
  - [ ] all tests pass
  - [ ] committed

#### Task T017: Transfer Form Page
- **Status**: pending
- **Dependencies**: T016, T008
- **Estimate**: M
- **DoD**:
  - [ ] Form fields: From Account (dropdown), To Account (dropdown), Amount (number), Description (textarea, optional)
  - [ ] Form validation: amount > 0, from ≠ to (client-side via React Hook Form)
  - [ ] shadcn/ui Form component for layout and error display
  - [ ] Submit button: disabled on invalid form or during request
  - [ ] Calls TransfersService.createTransfer()
  - [ ] Success: success toast shown + redirected to /transactions
  - [ ] Error: error toast shown (e.g., "Insufficient balance"), retry button enabled
  - [ ] Loading state: spinner in button, form disabled
  - [ ] Dark theme ready
  - [ ] Unit tests (validation, submit, error handling)
  - [ ] all tests pass
  - [ ] committed

#### Task T018: Toast Notifications
- **Status**: pending
- **Dependencies**: T017
- **Estimate**: S
- **DoD**:
  - [ ] Context provider for toast notifications (ToastContext)
  - [ ] Hook: useToast() with methods .success(message), .error(message), .info(message)
  - [ ] shadcn/ui Toaster component renders toasts in UI
  - [ ] Integrated in all API error responses (T020 frontend tests will use)
  - [ ] Auto-dismiss after 5 seconds
  - [ ] Maximum 3 toasts on screen at once
  - [ ] committed

---

### Phase 4: Testing (T019-T022)

#### Task T019: API Unit Tests - Controllers & Services
- **Status**: pending
- **Dependencies**: T010
- **Estimate**: L
- **DoD**:
  - [ ] xUnit test project with tests for all controllers and services
  - [ ] Happy path tests:
    - [ ] POST /auth with valid credentials → 200 + JWT
    - [ ] GET /accounts → 200 + accounts array
    - [ ] GET /transactions → 200 + paginated transactions
    - [ ] POST /transfers with valid data → 201 + transfer details
  - [ ] Error path tests:
    - [ ] POST /auth with invalid credentials → 401
    - [ ] GET /accounts without JWT → 401
    - [ ] GET /accounts/{id} with nonexistent id → 404
    - [ ] POST /transfers with insufficient balance → 400 + InsufficientBalance error code
  - [ ] TransferService tests: all validation rules covered
  - [ ] JwtTokenService tests: generation, validation, expiry
  - [ ] AuthService tests: happy path + invalid credentials
  - [ ] Edge case tests: balance = 0, transfer amount = balance, duplicate transfers
  - [ ] Code coverage >80% (API layer)
  - [ ] all tests pass (`dotnet test`)
  - [ ] committed

#### Task T020: Frontend Unit Tests - Components
- **Status**: pending
- **Dependencies**: T018
- **Estimate**: L
- **DoD**:
  - [ ] Vitest configured with React Testing Library and MSW (Mock Service Worker)
  - [ ] Login page tests:
    - [ ] Renders form with email + password inputs
    - [ ] Submit valid credentials → calls AuthService
    - [ ] API error → displays error message
  - [ ] Dashboard page tests:
    - [ ] Renders account cards on load
    - [ ] Loading state shows skeleton
    - [ ] API error → shows retry button
  - [ ] Transactions page tests:
    - [ ] Renders table with transactions
    - [ ] Filter by category → calls API with filter param
    - [ ] Pagination: next/prev buttons work
  - [ ] Transfer form tests:
    - [ ] Form validation (amount > 0, from ≠ to) prevents submit
    - [ ] Submit valid form → calls TransfersService
    - [ ] API error → displays error toast
  - [ ] API client tests: all services return correct types
  - [ ] Code coverage >70% (components)
  - [ ] all tests pass (`npm run test`)
  - [ ] committed

#### Task T021: Playwright E2E Tests - Dashboard Flow
- **Status**: pending
- **Dependencies**: T020
- **Estimate**: M
- **DoD**:
  - [ ] Playwright configured (chromium browser, config file)
  - [ ] Test setup: start API server + React dev server (or docker compose)
  - [ ] Test 1: Login flow
    - [ ] Navigate to /login
    - [ ] Enter demo credentials (admin@homebank.local / demo123)
    - [ ] Click login button
    - [ ] JWT stored in localStorage
    - [ ] Redirected to /dashboard
  - [ ] Test 2: Dashboard & Transactions
    - [ ] Dashboard displays account cards
    - [ ] Each card shows name, balance, last updated
    - [ ] Click account → navigates to /transactions
    - [ ] Transactions table shows rows
    - [ ] Category badge visible on each row
  - [ ] Test 3: Category filter
    - [ ] Select category from filter dropdown
    - [ ] Transactions table updates (filtered)
    - [ ] Unfilter → all transactions shown
  - [ ] Tests run reliably (no flakes)
  - [ ] all tests pass (`npm run e2e`)
  - [ ] committed

#### Task T022: Playwright E2E Tests - Transfer Flow
- **Status**: pending
- **Dependencies**: T021
- **Estimate**: M
- **DoD**:
  - [ ] Test 1: Happy path transfer
    - [ ] Login → /transfer page
    - [ ] Select source & destination accounts
    - [ ] Enter amount + optional description
    - [ ] Click submit
    - [ ] Success toast shown
    - [ ] Redirected to /transactions
    - [ ] New transaction visible in both accounts
  - [ ] Test 2: Invalid scenarios
    - [ ] Transfer with insufficient balance → error toast, not redirected
    - [ ] Transfer from account to same account → form validation prevents submit
    - [ ] Enter negative amount → form validation prevents submit
  - [ ] Test 3: Transfer verification
    - [ ] Source account balance decreases
    - [ ] Destination account balance increases
    - [ ] Transaction timestamps recorded
  - [ ] Tests run reliably (no race conditions)
  - [ ] all tests pass (`npm run e2e`)
  - [ ] committed

---

### Phase 5: Infrastructure & CI/CD (T023-T025)

#### Task T023: Docker Compose Setup
- **Status**: pending
- **Dependencies**: T010, T018
- **Estimate**: M
- **DoD**:
  - [ ] docker-compose.yml with 2 services: api, web
  - [ ] API service: builds Dockerfile.api (multi-stage, .NET 9 runtime, ~200MB image)
  - [ ] Web service: builds Dockerfile.web (Node 22, npm build, nginx, ~100MB image)
  - [ ] Environment variables: API_URL (http://api:5000), JWT_SECRET
  - [ ] Healthchecks configured: GET /health for API, GET / for web
  - [ ] Network: web can reach API at http://api:5000
  - [ ] `docker compose up` starts both services, all ready within 30s
  - [ ] `docker compose down` cleans up containers and volumes
  - [ ] Dockerfile.api optimized (multi-stage, no test layer in final image)
  - [ ] Dockerfile.web optimized (nginx serves static files, gzip enabled)
  - [ ] committed

#### Task T024: GitHub Actions CI Pipeline
- **Status**: pending
- **Dependencies**: T023
- **Estimate**: M
- **DoD**:
  - [ ] .github/workflows/ci.yml created
  - [ ] Trigger: on PR (pull_request), on push to main (push)
  - [ ] Jobs:
    - [ ] build-api: .NET build, unit tests, StyleCop lint
    - [ ] build-web: Node build, unit tests, ESLint lint
    - [ ] e2e-tests: docker compose up, Playwright tests
  - [ ] All jobs run in parallel (no unnecessary dependencies)
  - [ ] Build failures prevent merge (required status checks)
  - [ ] Test coverage reports (optional: codecov integration)
  - [ ] CI passes locally first (tested on developer machine)
  - [ ] PR cannot merge if CI fails (branch protection rule recommended)
  - [ ] committed

#### Task T025: Documentation
- **Status**: pending
- **Dependencies**: T024
- **Estimate**: M
- **DoD**:
  - [ ] spec.md complete: requirements, architecture, tech stack, design decisions
  - [ ] README.md updated with:
    - [ ] Project overview
    - [ ] Local dev setup (clone, restore, npm install)
    - [ ] Run locally (Vite dev server + dotnet run)
    - [ ] Run with Docker (docker compose up)
    - [ ] Run unit tests (dotnet test, npm run test)
    - [ ] Run E2E tests (npm run e2e)
    - [ ] Folder structure explanation
    - [ ] API contracts (examples: POST /auth, GET /accounts, POST /transfers)
  - [ ] API documentation in Scalar (auto-generated from OpenAPI attributes)
  - [ ] Contributing guide: PR checklist, code style, testing requirements
  - [ ] Deployment guide: future DB migration path, production checklist
  - [ ] TECH_STACK.md: versions of all dependencies with rationale
  - [ ] committed

---

## Dependencies Graph

```
T001 (Scaffolding)
├─ T002 (DbContext)
│  ├─ T003 (Models)
│  │  ├─ T004 (JWT)
│  │  │  ├─ T005 (GET accounts)
│  │  │  │  ├─ T006 (GET transactions)
│  │  │  │  │  ├─ T007 (Transfer validation)
│  │  │  │  │  │  ├─ T008 (POST transfer)
│  │  │  │  │  │  │  ├─ T019 (API unit tests)
│  │  │  │  │  │  │  ├─ T021 (E2E Dashboard)
│  │  │  │  │  │  │  │  └─ T022 (E2E Transfer)
│  │  │  │  │  │  │  ├─ T023 (Docker)
│  │  │  │  │  │  │  │  ├─ T024 (GitHub Actions)
│  │  │  │  │  │  │  │  │  └─ T025 (Docs)
├─ T009 (Health)
├─ T010 (Error handling)

T011 (React setup) [parallel to T001]
├─ T012 (API client)
│  ├─ T013 (Layout)
│  │  ├─ T014 (Login)
│  │  │  ├─ T015 (Dashboard)
│  │  │  │  ├─ T016 (Transactions)
│  │  │  │  │  ├─ T017 (Transfer form)
│  │  │  │  │  │  ├─ T018 (Toasts)
│  │  │  │  │  │  │  ├─ T020 (Frontend unit tests)
│  │  │  │  │  │  │  │  └─ T021 (E2E Dashboard)
```

---

## Implementation Timeline

| Week | Focus | Tasks | Deliverables |
|------|-------|-------|--------------|
| **Week 1** | API Foundation | T001-T010 | .NET solution with all REST endpoints, unit tests, health check |
| **Week 2** | Frontend & E2E | T011-T022 | React app with UI, unit tests, E2E tests (dashboard + transfer) |
| **Week 3** | Infrastructure | T023-T025 | Docker Compose, GitHub Actions CI, documentation |

**Deployment Ready**: End of Week 3

---

## Success Criteria

- [ ] Solution builds cleanly: `dotnet build` (no errors/warnings)
- [ ] All API unit tests pass: `dotnet test` (>80% coverage)
- [ ] All frontend unit tests pass: `npm run test` (>70% coverage)
- [ ] All E2E tests pass: `npm run e2e` (3 critical flows)
- [ ] Docker compose runs locally: `docker compose up` (all services healthy in <30s)
- [ ] GitHub Actions CI passes on PR
- [ ] spec.md + README.md complete and accurate
- [ ] Code follows C# conventions (StyleCop >0 warnings)
- [ ] Code follows TypeScript + React best practices (ESLint >0 warnings)
- [ ] All PRs follow Definition of Done framework

---

## Execution Rules

1. **One task at a time**: Mark as in-progress, complete all DoD items, commit, mark as completed
2. **Atomic commits**: Each task = one commit (no mega-commits)
3. **Branch per task**: `feature/T00X-task-title` (keeps history clean)
4. **Commit message**: `feat(T00X): task title` or `test(T00X): task title`
5. **All tests before commit**: `dotnet test` + `npm run test` (in respective projects)
6. **No TypeScript errors**: `tsc --noEmit` passes
7. **No lint warnings**: `dotnet build` reports no StyleCop issues; `npm run lint` reports no ESLint issues

---

**Ready to execute? Pick T001 and start building! 🚀**
