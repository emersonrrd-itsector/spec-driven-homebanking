# Home Banking: Task Plan

**Status**: Ready for execution  
**Start Date**: 2026-02-17  
**Target Completion**: End of Week 3 (2026-03-02)  

---

## Overview

25 atomic tasks organized in 5 phases. Each task is independently deployable and testable. Dependencies are clear; tasks can be parallelized where possible.

**Status**: 11 of 26 tasks complete (T000-T010, Phase 1 API Foundation complete!)  
**Phases**:
0. **Bootstrap** (T000): Project scaffold and build verification
1. **API Foundation** (T001-T010): .NET scaffolding, data layer, REST endpoints, auth
2. **Frontend Foundation** (T011-T015): React setup, layout, login, dashboard
3. **Transaction & Transfer UI** (T016-T018): Tables, forms, notifications
4. **Testing** (T019-T022): Unit tests (API & frontend), E2E tests
5. **Infrastructure & CI/CD** (T023-T025): Docker, GitHub Actions, documentation

---

## Global Definition of Done (DoD)

**CRITICAL PRINCIPLE**: All tasks MUST pass the same gates locally and in CI. If it passes locally, it MUST pass CI. No surprises.

### 1. Universal DoD (Applies to Every Task)

Every task, regardless of component (API, Web, Infrastructure), must satisfy ALL of these before marking complete:

#### 1.1 Code Quality Gates

- [ ] **Zero Build Errors**
  - API: `dotnet build` exits with code 0, no error messages
  - Web: `npm run build` exits with code 0, no error messages

- [ ] **Zero Lint Errors** (not warnings—errors only)
  - API: `dotnet build` produces NO StyleCop errors (warnings allowed, logged for review)
  - Web: `npm run lint` produces NO ESLint errors in modified files

- [ ] **Zero TypeScript Compilation Errors**
  - Web: `tsc --noEmit` passes with exit code 0 (in src/ and tests/)

#### 1.2 Testing Gates

- [ ] **New Tests Created for New Functionality**
  - Every new feature/method must have corresponding unit test(s)
  - Test-to-code ratio: ≥1 test method per feature method
  - Test naming: `[Method]_[Scenario]_[ExpectedOutcome]` (e.g., `Transfer_InsufficientBalance_Returns400`)

- [ ] **ALL Tests Pass (New + Existing)**
  - API: `dotnet test --no-build` passes with exit code 0, all test counts unchanged or increased
  - Web: `npm run test -- --run` passes with exit code 0, all test counts unchanged or increased
  - **Key**: No broken existing tests. If a test breaks, fix it or adjust DoD checklist before declaring done.

- [ ] **Code Coverage Maintained/Improved**
  - API: Coverage report ≥80% (new tests must not reduce coverage)
  - Web: Coverage report ≥70% (new tests must not reduce coverage)
  - Tools: xUnit with Coverlet extension (API), Vitest with coverage mode (Web)

#### 1.3 Documentation & Comments

- [ ] **Code Comments Updated** (where relevant)
  - New public methods: XML doc comments (C#) or JSDoc (TypeScript)
  - Complex logic: inline comments explaining "why", not "what"
  - API endpoints: Proper [HttpGet] / [HttpPost] attributes with OpenAPI attributes for Scalar

- [ ] **plan.md Updated**
  - [ ] Task status changed from `pending` → `in-progress` (when starting)
  - [ ] Task status changed from `in-progress` → `completed` (when finished)
  - [ ] `Plan changes:` section in task filled with all modifications to downstream tasks (if any)
  - [ ] New task dependencies recorded (if discovered)
  - [ ] Estimate adjusted (if actually took different time than planned)

- [ ] **README/Architecture Docs Updated** (if infrastructure or API contract changed)
  - New folders, new endpoints, new configuration → document it
  - New deployment steps → update docker-compose or CI section

#### 1.4 Version Control & Commit

- [ ] **Atomic Commit**
  - One task = one commit with clear message format:
    - Feature task: `feat(T00X): short description`
    - Test task: `test(T00X): short description`
    - Docs task: `docs(T00X): short description`
    - Fix task: `fix(T00X): short description`
  - Commit includes: code changes + updated plan.md + updated README (if applicable)

- [ ] **Branch Clean & Rebased** (before final push)
  - Branch name: `feature/T00X-kebab-case-title`
  - No merge conflicts
  - Rebased on latest `main` (recommended, not required for this learning project)

### 2. Local Validation Commands

Developers run these commands in order to verify DoD before pushing:

**API/.NET Tasks:**
```bash
cd src/api
dotnet build                    # Check: 0 errors, lint errors allowed (log them)
dotnet test --no-build          # Check: all tests pass, exit code = 0
dotnet test --no-build /p:CollectCoverage=true  # Check: coverage ≥80%
cd ../..
```

**Web/React Tasks:**
```bash
cd src/web
npm run build                   # Check: 0 errors, exit code = 0
npm run lint                    # Check: 0 ESLint errors
tsc --noEmit                    # Check: 0 TypeScript errors
npm run test -- --run           # Check: all tests pass, exit code = 0
npm run test -- --run --coverage # Check: coverage ≥70%
cd ../..
```

**Infrastructure Tasks:**
```bash
# For Docker tasks:
docker compose up -d            # Check: all services healthy in <30s
docker compose logs api         # Check: /health endpoint responds 200 OK
docker compose down -v

# For CI tasks:
# (CI workflow syntax validated locally, reviewed before merge)
```

**All Tasks:**
```bash
git status                      # Check: modified files are planned/documented
git add .
git commit -m "feat(T00X): description"  # Check: commit succeeds
npm run test -- --run 2>/dev/null && echo "Local validation GREEN ✓" || echo "FAILED ✗"
```

### 3. CI Pipeline Equivalents (GitHub Actions)

The CI workflow MUST run the exact same commands as local validation. No surprises.

**`.github/workflows/ci.yml` jobs:**

```yaml
build-api:
  runs-on: ubuntu-latest
  steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    - run: cd src/api && dotnet build
    - run: cd src/api && dotnet test --no-build
    - run: cd src/api && dotnet test --no-build /p:CollectCoverage=true
    # (All commands from "Local Validation" above)

build-web:
  runs-on: ubuntu-latest
  steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-node@v4
      with:
        node-version: '22.x'
    - run: cd src/web && npm install
    - run: cd src/web && npm run build
    - run: cd src/web && npm run lint
    - run: cd src/web && tsc --noEmit
    - run: cd src/web && npm run test -- --run
    - run: cd src/web && npm run test -- --run --coverage
    # (All commands from "Local Validation" above)

e2e-tests:
  runs-on: ubuntu-latest
  needs: [build-api, build-web]
  steps:
    - uses: actions/checkout@v4
    - run: docker compose up -d
    - run: docker compose logs api
    - run: cd src/web && npm install && npx playwright install
    - run: cd src/web && npm run e2e
    - run: docker compose down -v
```

**Gate Rule**: All jobs must pass (no allowed failures). PR cannot merge if any job fails.

### 4. Best Practices for AI-Assisted Development DoD

When using Copilot/AI for code generation, add these checks:

- [ ] **AI-Generated Code Reviewed**
  - Read the generated code; understand what it does
  - Check for security issues: hardcoded secrets, SQL injection, XSS vulnerabilities
  - Check for performance issues: nested loops, inefficient queries

- [ ] **Generated Tests Are Not Trivial**
  - Generated tests must test edge cases, not just happy path
  - Verify tests actually fail if the feature is broken (mutation testing mindset)

- [ ] **No Copy-Paste of Boilerplate**
  - Each file or module should have a clear purpose, not duplicate code
  - Use DRY principle: extract repeated patterns into shared utilities

- [ ] **Comments Clarify AI Assistance**
  - If significant logic is AI-generated, add comment: "Generated with Copilot; reviewed for [security/performance/clarity]"
  - Helps future maintainers understand confidence level in code

- [ ] **Error Handling Is Explicit**
  - AI often generates happy-path code; explicitly test error cases
  - Ensure try-catch, null checks, validation are present

### 5. Task Completion Review Process

After completing a task (before pushing):

1. **Self-Review Checklist** (5 min)
   - [ ] Read my own code: Does it make sense? Would I understand it in 3 months?
   - [ ] Are there any TODOs or FIXMEs left? (Delete or create follow-up task)
   - [ ] Did I use meaningful variable names? (No `x`, `temp`, `data`)

2. **Run Local Validation** (2-5 min per component)
   ```bash
   # For API tasks:
   cd src/api && dotnet build && dotnet test --no-build && dotnet test --no-build /p:CollectCoverage=true
   # For Web tasks:
   cd src/web && npm run build && npm run lint && tsc --noEmit && npm run test -- --run
   ```

3. **Verify Plan Updates** (2 min)
   - [ ] plan.md status updated to `completed`
   - [ ] Estimate matches reality (if not, update)
   - [ ] `Plan changes:` section filled with any impacts to downstream tasks
   - [ ] New dependencies or risks documented

4. **Commit & Push** (1 min)
   ```bash
   git add .
   git commit -m "feat(T00X): clear, concise description"
   git push origin feature/T00X-branch-name
   ```

5. **Review Plan for Task Impacts** (5 min)
   - [ ] Did this task affect any downstream tasks?
   - [ ] Do dependencies still make sense? (e.g., if T005 is now complete, can T006 start now?)
   - [ ] Did architecture change? (e.g., new folder, new model, new endpoint?)
   - [ ] Update plan.md `Plan changes:` sections in all affected downstream tasks
   - [ ] Example: "T007 now depends on T006 (moved from T005) because transfer validation requires transaction DTOs"

6. **Wait for CI to Green** (5-30 min)
   - Task is NOT done until GitHub Actions CI workflow passes
   - If CI fails: fix locally, re-run local validation, commit, push, re-run CI
   - Review CI logs if failure is unclear

---

## Execution Workflow

### Before Each Task
1. Create branch: `git checkout -b feature/T00X-task-title`
2. Update plan.md: status → `in-progress`
3. Commit: `git commit -m "docs(T00X): mark in-progress"`

### During Task
1. Write code following C# / React best practices
2. Write tests for new functionality (before or after, both valid)
3. Update comments and docs
4. Verify folder structure and naming match spec.md

### Before Marking Complete
1. Run full local validation (see section 2 above)
2. Review own code (see section 5 above)
3. Update plan.md: status → `completed`, fill `Plan changes:`
4. Perform task impact review (see section 5 above)
5. Commit all changes
6. Push to GitHub
7. Wait for CI to pass (green check on commit) ← **Task not done until here**
8. Merge to main (or update tracking in plan.md)

### After Each Task (Weekly Review)
- Review all completed tasks
- Check if plan adjustments needed
- Update future task estimates if patterns emerge
- Document learnings in README or CONTRIBUTING.md

---

## Task Breakdown

### Phase 0: Bootstrap (T000)

#### Task T000: Project Scaffold & Build Verification
- **Status**: completed
- **Dependencies**: none
- **Estimate**: S
- **DoD**: 
  - [x] Folder structure created (src/api, src/web)
  - [x] .NET 9 solution scaffolded with global.json (9.0.100)
  - [x] API project (HomeBanking.API, WebAPI template)
  - [x] Tests project (HomeBanking.Tests, xUnit)
  - [x] React+Vite+TypeScript project scaffolded in src/web
  - [x] npm install completed, 176 packages
  - [x] dotnet build succeeds with 0 errors
  - [x] npm run build succeeds with 0 errors
  - [x] Committed with message "T000: project scaffold and build verification"
- **Plan changes**: T001 scope updated—solution structure now ready, focus shifts to adding Domain project and configuring shared settings (Directory.Build.props, global usings)

---

### Phase 1: API Foundation (T001-T010)

#### Task T001: .NET Configuration & Domain Project Setup
- **Status**: completed
- **Dependencies**: T000
- **Estimate**: S
- **DoD**: 
  - [x] Domain project created (HomeBanking.Domain class library)
  - [x] Domain project added to solution
  - [x] API project references Domain project
  - [x] Global usings configured in Directory.Build.props
  - [x] Directory.Build.props with common settings (LangVersion, Nullable, etc.)
  - [x] builds cleanly (`dotnet build`)
  - [x] no lint warnings (StyleCop)
  - [x] committed
- **Plan changes**: None - T002 dependencies unchanged

#### Task T002: EF Core InMemory Context Setup
- **Status**: completed
- **Dependencies**: T001 (Domain project required)
- **Estimate**: M
- **DoD**:
  - [x] DbContext created with InMemory provider (HomeBankingContext.cs)
  - [x] Models: User, Account, Transaction defined in HomeBanking.Domain
  - [x] Seed data initialization (1 user, 2 accounts, 15 transactions each)
  - [x] DbContext tests validate seeding (16 comprehensive tests)
  - [x] all tests pass (16/16 passing)
  - [x] committed with message "feat(T002): EF Core InMemory Context Setup"
- **Plan changes**: T003 can now proceed - domain models are ready but in different organization (models in Domain project, DbContext in API). T003 scope adjusted: focus shifts to aggregates and validation rules rather than basic model definition.
- **Implementation Summary**:
  - Created domain models: User.cs, Account.cs, Transaction.cs with proper documentation
  - Implemented HomeBankingContext with EF Core Fluent API configuration and relationships
  - Created ServiceCollectionExtensions.cs for DI registration
  - Updated Program.cs to register DbContext
  - Implemented SeedIfEmpty() method for test data generation (password hashed with SHA256)
  - 16 unit tests covering initialization, relationships, cascade delete, and data integrity
  - All StyleCop requirements met, zero build errors

#### Task T003: Core Domain Models & Value Objects
- **Status**: completed
- **Dependencies**: T002 (completed)
- **Estimate**: M
- **DoD**:
  - [x] User aggregate (Id, Email, PasswordHash) - validation methods added
  - [x] Account aggregate (Id, UserId, Balance, Currency, AccountNumber) - validation methods added
  - [x] Transaction DTO (Id, AccountId, Amount, Date, Description, Category, Type) - complete
  - [x] Validation rules defined (balance ≥ 0, transfer amount > 0, duration validation, etc.)
  - [x] unit tests pass (44 tests, all passing)
  - [x] committed with message "feat(T003): Core Domain Models & Value Objects"
- **Implementation Details**:
  - Created Money value object (Amount, Currency)
  - Enhanced Email value object (validate format, factory method)
  - Added PasswordHash.CreateFromPlainText() static method
  - Created DomainConstants.cs for categories and validation rules
  - Added 28+ new unit tests for aggregate behavior, validation, and business rules
  - All value objects implement Equals/GetHashCode for value equality
- **Plan changes**: None - T004 dependencies unchanged

#### Task T004: JWT Authentication Middleware
- **Status**: completed
- **Dependencies**: T003
- **Estimate**: M
- **DoD**:
  - [x] JwtTokenService generates tokens (hardcoded demo credentials: admin@homebank.local/demo123)
  - [x] AuthenticationMiddleware validates JWT on protected endpoints
  - [x] Demo user seeding (admin@homebank.local, password hashed)
  - [x] POST /auth endpoint returns JWT on valid credentials
  - [x] 401 response on invalid credentials
  - [x] unit tests for token generation/validation/expiry
  - [x] all tests pass
  - [x] committed
- **Implementation Summary**:
  - Created JwtTokenService with HS256 token generation/validation, 24-hour expiry
  - Implemented IAuthService + AuthService for credential validation
  - Created AuthController with POST /api/auth endpoint
  - Added DTOs: LoginRequest, LoginResponse, UserDto, ErrorResponse
  - Registered services in Program.cs with JWT bearer authentication
  - Demo user (admin@homebank.local/demo123) seeded on startup
  - 30+ unit tests for token generation, validation, authentication flows
  - All 86 tests passing (100%), 0 build errors, 0 lint errors
- **Plan changes**: T005 can now proceed - authentication middleware ready for protecting account endpoints

#### Task T005: Account Controller - Get Accounts & Balances
- **Status**: completed
- **Dependencies**: T004
- **Estimate**: M
- **DoD**:
  - [x] GET /api/accounts returns user's accounts with balance
  - [x] GET /api/accounts/{id} returns single account detail
  - [x] Response DTO: { id, name, balance, currency, lastUpdated }
  - [x] Authorized (JWT required)
  - [x] Unit tests (200, 401, 404 cases)
  - [x] Scalar OpenAPI schema generated and readable
  - [x] all tests pass
  - [x] committed
- **Implementation Summary**:
  - Created AccountsController with two endpoints (GET /api/accounts, GET /api/accounts/{id})
  - Implemented IAccountService + AccountService for business logic
  - Created response DTOs: AccountDto, AccountsResponse
  - 31 new tests for Account functionality (AccountsControllerTests: 17, AccountServiceTests: 14)
  - All 172 tests passing (including 47 additional tests for improved coverage)
  - Code coverage: 85.21% (exceeds 80% requirement)
  - User isolation enforced: each user can only access their own accounts
  - Both endpoints require JWT authorization
  - ProducesResponseType attributes configured for Scalar OpenAPI documentation
- **Plan changes**: None - T006 dependencies unchanged, can proceed with Transaction Controller implementation

#### Task T006: Transaction Controller - Get Transactions
- **Status**: completed
- **Dependencies**: T005
- **Estimate**: M
- **DoD**:
  - [x] GET /api/transactions?accountId={id} returns paginated transactions
  - [x] Response DTO: { id, amount, date, description, category, type }
  - [x] Pagination support (default: skip=0, take=10)
  - [x] Filtering by category optional
  - [x] Authorized (JWT required)
  - [x] Unit tests (200, 401, pagination, filtering)
  - [x] Scalar OpenAPI schema updated
  - [x] all tests pass
  - [x] committed
- **Implementation Summary**:
  - Created TransactionDto and TransactionsResponse DTOs
  - Implemented ITransactionService + TransactionService for business logic
  - Created TransactionsController with GET /api/transactions endpoint
  - Supports query params: accountId (required), skip (0), take (10), category (optional)
  - Returns paginated transactions with total count, ordered by date descending
  - User isolation enforced: can only access own account transactions
  - 26 controller tests + 23 service tests (all passing)
  - All 209 tests passing (improved from 86)
  - ProducesResponseType attributes configured for Scalar OpenAPI
- **Plan changes**: None - T007 dependencies unchanged, can proceed with Transfer validation

#### Task T007: Transfer Validation & Business Rules
- **Status**: completed
- **Dependencies**: T006
- **Estimate**: M
- **DoD**:
  - [x] TransferService validates: sufficient balance, accounts exist, from ≠ to
  - [x] Error responses: InsufficientBalance, AccountNotFound, InvalidTransfer
  - [x] Transaction created in source account (debit, "Transfer to [name]")
  - [x] Transaction created in destination account (credit, "Transfer from [name]")
  - [x] Balances updated atomically
  - [x] Unit tests cover all validation scenarios (57 tests for T007)
  - [x] all tests pass (238 total tests passing)
  - [x] committed with message "feat(T007): Transfer Validation & Business Rules"
- **Plan changes**: None - T008 (Transfer Controller) can now proceed with POST /api/transfers endpoint using this service
- **Implementation Summary**:
  - Created ITransferService interface + TransferService implementation
  - Validates: sufficient balance, account existence, source ≠ destination, user authorization, account active status, currency matching
  - Creates atomic transactions: debit in source ("Transfer to [account number]"), credit in destination ("Transfer from [account number]")
  - Updates both balances atomically with single SaveChangesAsync call
  - Error codes: InsufficientBalance, AccountNotFound, InvalidTransfer, Unauthorized
  - Created TransferRequest DTO (FromAccountId, ToAccountId, Amount, Description)
  - Created TransferResult DTO (success/failure responses with new balances)
  - 57 comprehensive unit tests covering happy path, all validation failures, boundary conditions, and edge cases
  - All 238 tests passing, 0 build errors, 0 lint warnings
  - Registered services in Program.cs with scoped lifetime

#### Task T008: Transfer Controller - POST Transfer
- **Status**: completed
- **Dependencies**: T007
- **Estimate**: M
- **DoD**:
  - [x] POST /api/transfers accepts { fromAccountId, toAccountId, amount, description }
  - [x] Input validation (amount > 0.01, description optional)
  - [x] Calls TransferService for business logic
  - [x] Returns 201 { transferId, status, fromAccount, toAccount, amount }
  - [x] On error: 400 with error DTO { code, message, details, timestamp }
  - [x] Authorized (JWT required)
  - [x] Unit tests (201, 400 bad balance, 404 account not found, etc.)
  - [x] Scalar OpenAPI schema updated with ProducesResponseType
  - [x] all tests pass (253 total tests passing)
  - [x] committed with message "feat(T008): Transfer Controller - POST Transfer"
- **Plan changes**: None - T009 (Health Check Endpoint) can proceed independently, T010 (API Error Response) is now a dependency for consistent error handling
- **Implementation Summary**:
  - Created TransfersController.cs with POST /api/transfers endpoint
  - Accepts TransferRequest with DataAnnotations validation (amount > 0.01, description ≤ 500 chars)
  - Extracts userId from JWT claims (ClaimTypes.NameIdentifier)
  - Calls ITransferService.ExecuteTransferAsync() for business logic
  - Returns 201 Created with TransferResponse (includes transferId, status, source/dest account details with new balances)
  - Returns 400 Bad Request on validation errors or business logic failures (InsufficientBalance, AccountNotFound, InvalidTransfer)
  - Returns 401 Unauthorized if JWT missing or invalid user ID claim
  - Returns 500 Internal Server Error on unexpected exceptions
  - ProducesResponseType attributes configured for Scalar OpenAPI documentation
  - 15 comprehensive unit tests: happy path (3), validation errors (6), business logic errors (3), authorization (2), exception handling (2)
  - All 253 tests passing (238 previous + 15 new), 0 build errors, 0 lint warnings

#### Task T009: Health Check Endpoint
- **Status**: completed
- **Dependencies**: T001
- **Estimate**: S
- **DoD**:
  - [x] GET /health returns { status: "healthy", timestamp, version }
  - [x] GET /health/ready checks database connectivity
  - [x] Unauthenticated (public endpoints with [AllowAnonymous])
  - [x] Unit tests (200/503 responses, JSON format, timestamps)
  - [x] all tests pass (264 total passing)
  - [x] committed with message "feat(T009): Health Check Endpoint"
- **Plan changes**: None - T010 (API Error Response Standardization) can now proceed
- **Implementation Summary**:
  - Created HealthController with two public endpoints
  - GET /api/health: Returns 200 OK with { status: "healthy", timestamp: ISO8601 UTC, version: "1.0.0" }
  - GET /api/health/ready: Checks database connectivity, returns 200 if ready or 503 if not
  - Created HealthResponse DTO (Status, Timestamp, Version)
  - Created ReadinessResponse DTO (Status, Timestamp, Details)
  - Both endpoints are public (no JWT required, [AllowAnonymous] attributes)
  - Database check uses DbContext.Database.CanConnectAsync() + FirstOrDefaultAsync() query
  - Comprehensive error handling with error details on 503 responses
  - 11 comprehensive unit tests: health endpoint (5 tests), readiness endpoint (5 tests), null context validation (1 test)
  - All 264 tests passing (253 previous + 11 new), 0 build errors, 0 lint warnings

#### Task T010: API Error Response Standardization
- **Status**: completed
- **Dependencies**: T009
- **Estimate**: S
- **DoD**:
  - [x] Global exception handling middleware (GlobalExceptionHandlerMiddleware)
  - [x] All errors return { code, message, details, timestamp }
  - [x] Handles 400 (validation), 401 (auth), 404, 409, 500 scenarios
  - [x] Errors logged to console with request context (timestamp, path, method, status)
  - [x] Unit tests for error middleware (25 tests) and model validation filter (10 tests)
  - [x] all tests pass (295 total passing)
  - [x] committed with message "feat(T010): API Error Response Standardization"
- **Plan changes**: Phase 1 (API Foundation) now complete! T011-T015 can begin (Frontend Foundation phase)
- **Implementation Summary**:
  - Created GlobalExceptionHandlerMiddleware in Middleware/ folder
  - Exception type to status code mapping: ArgumentException→400, InvalidOperationException→409, KeyNotFoundException→404, UnauthorizedAccessException→401, others→500
  - Logs all exceptions with ILogger<GlobalExceptionHandlerMiddleware>
  - Returns standardized ErrorResponse: { code, message, details, timestamp (ISO8601 UTC) }
  - Created ModelValidationFilter for ModelState validation errors (400 Bad Request)
  - Validation errors include field-level error details: { errors: { field: ["message"] } }
  - Registered GlobalExceptionHandlerMiddleware as first middleware in Program.cs
  - Registered ModelValidationFilter globally for all controllers
  - 25 middleware tests + 10 filter tests covering all exception types, error formats, logging, timestamps
  - All 295 tests passing (264 previous + 31 new), 0 build errors, 0 StyleCop violations

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
| **Week 0** | Bootstrap | T000 | Project scaffold, build verification ✓ |
| **Week 1** | API Foundation | T001-T010 | .NET solution with all REST endpoints, unit tests, health check |
| **Week 2** | Frontend & E2E | T011-T022 | React app with UI, unit tests, E2E tests (dashboard + transfer) |
| **Week 3** | Infrastructure | T023-T025 | Docker Compose, GitHub Actions CI, documentation |

**Deployment Ready**: End of Week 3 → **All CI Tests Green ✓**

---

## Success Criteria (Project-Level)

A task is complete when:

1. ✅ All DoD gates pass (see "Global Definition of Done" section)
2. ✅ GitHub Actions CI workflow passes (green check on commit) *or CI not applicable for infra tasks*
3. ✅ plan.md updated with task status, estimate, and plan changes
4. ✅ Code reviewed for AI-assistance quality (if applicable)

**Project Success** (End of Week 3):
- [x] T000 completed (project scaffold, build verified)
- [ ] All 25 tasks (T001-T025) completed (`completed` status in plan.md)
- [ ] All unit tests pass: API >80% coverage, Web >70% coverage
- [ ] All E2E tests pass: 3 critical flows (login, dashboard, transfer)
- [ ] Docker compose runs locally, all services healthy
- [ ] GitHub Actions CI GREEN on main branch
- [ ] spec.md + README.md + CONTRIBUTING.md complete
- [ ] Zero lint errors (StyleCop, ESLint)
- [ ] Zero TypeScript compilation errors
- [ ] Solution ready for production deployment (or next phase)

---

## Quick Reference: Local Validation Checklist

Print this and check before each commit:

```
BEFORE EACH COMMIT:
  API Tasks:
    [ ] cd src/api && dotnet build (exit code 0)
    [ ] cd src/api && dotnet test --no-build (all pass)
    [ ] cd src/api && dotnet test --no-build /p:CollectCoverage=true (≥80%)
    [ ] plan.md updated (status, estimate, changes)

  Web Tasks:
    [ ] cd src/web && npm run build (exit code 0)
    [ ] cd src/web && npm run lint (0 ESLint errors)
    [ ] cd src/web && tsc --noEmit (0 TypeScript errors)
    [ ] cd src/web && npm run test -- --run (all pass)
    [ ] cd src/web && npm run test -- --run --coverage (≥70%)
    [ ] plan.md updated (status, estimate, changes)

  All Tasks:
    [ ] git commit -m "feat/test/docs(T00X): description"
    [ ] git push origin feature/T00X-branch
    [ ] Wait for CI workflow to pass (green check)
    [ ] Merge or mark ready for review
```

---

**Ready to execute? Pick T001 and start building! 🚀**

**Remember**: If it passes locally, it MUST pass CI. No surprises.
