# E2E Testing Guide

## Overview

This directory contains end-to-end (E2E) tests for the HomeBanking web application using Playwright.

## Test Files

### `login.spec.ts`
Tests the authentication flow:
- ✅ Successful login with valid credentials
- ✅ JWT token storage in localStorage
- ✅ Redirect to dashboard after login
- ✅ Error handling for invalid credentials
- ✅ Form validation (required fields, email format)
- ✅ Authentication persistence across page reloads

### `dashboard.spec.ts`
Tests the dashboard page:
- ✅ Welcome message with user email
- ✅ Display account cards with name, balance, last updated
- ✅ Navigate to transactions when clicking account card
- ✅ Responsive grid layout for multiple accounts
- ✅ Hover effects on cards
- ✅ Loading skeleton states
- ✅ Error handling with retry button

### `transactions.spec.ts`
Tests the transactions page and filtering:
- ✅ Display transactions table with all columns (Date, Description, Category, Amount)
- ✅ Category badges with colors
- ✅ Amount formatting with +/- signs and currency
- ✅ Filter transactions by category
- ✅ Clear filter to show all transactions
- ✅ Pagination controls and navigation
- ✅ Page size selector (10/25/50)
- ✅ Maintain filter when changing pages
- ✅ Empty state handling

### `helpers.ts`
Common test utilities and helper functions:
- `login()` - Authenticate user
- `navigateToTransactions()` - Go to transactions page
- `waitForTransactionsTable()` - Wait for table to load
- `getTransactionRows()` - Get table rows
- `getAccountCards()` - Get dashboard account cards
- `hasValidJwtToken()` - Check JWT storage
- `VALID_CATEGORIES` - List of transaction categories

## Prerequisites

### 1. API Server Running
The E2E tests require the API server to be running on `http://localhost:5000`.

**Option A: Run API locally**
```bash
cd src/api/HomeBanking.API
dotnet run
```

**Option B: Use Docker (if available)**
```bash
docker-compose up -d api
```

### 2. Install Dependencies
```bash
npm install
```

### 3. Install Playwright Browsers
```bash
npx playwright install chromium
```

## Running Tests

### Run all E2E tests
```bash
npm run e2e
```

### Run tests in headed mode (see browser)
```bash
npx playwright test --headed
```

### Run specific test file
```bash
npx playwright test login.spec.ts
```

### Run tests in debug mode
```bash
npx playwright test --debug
```

### Run tests in specific browser
```bash
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

### Run tests with UI mode
```bash
npx playwright test --ui
```

## Configuration

Playwright configuration is in [`playwright.config.ts`](../../playwright.config.ts):

- **Test directory**: `./src/e2e`
- **Base URL**: `http://localhost:5173`
- **Web server**: Automatically starts Vite dev server (`npm run dev`)
- **Browsers**: Chromium, Firefox, WebKit
- **Retries**: 2 in CI, 0 locally
- **Reporter**: HTML report

## Test Environment

### Demo Credentials
- **Email**: `admin@homebank.local`
- **Password**: `demo123`

### API Endpoints Used
- `POST /api/auth` - Login
- `GET /api/accounts` - Get accounts
- `GET /api/transactions` - Get transactions

### localStorage Keys
- `homebank_jwt` - JWT authentication token

## Best Practices

1. **Test Independence**: Each test is independent and can run in any order
2. **Authentication**: Tests clear localStorage before login to ensure clean state
3. **Waits**: Tests use proper Playwright waits (no arbitrary timeouts except where noted)
4. **Selectors**: Tests prefer accessible selectors (`getByRole`, `getByLabel`) over CSS classes
5. **Error Handling**: Tests gracefully handle loading states and API delays

## CI/CD Integration

Tests are configured to run in CI with:
- `forbidOnly: true` - Prevents `.only` tests in CI
- `retries: 2` - Retry flaky tests twice
- `workers: 1` - Run tests serially in CI to avoid conflicts

## Troubleshooting

### Tests fail with "API not found" errors
- Ensure API server is running on `http://localhost:5000`
- Check API health: `curl http://localhost:5000/api/accounts`

### Tests timeout
- Increase timeout in test file: `test.setTimeout(60000)`
- Check if API is slow or overloaded

### Browser not opening
- Install browsers: `npx playwright install`
- Try specific browser: `npx playwright test --project=chromium`

### Failed snapshots
- Update snapshots: `npx playwright test --update-snapshots`

## View Test Reports

After running tests, view the HTML report:
```bash
npx playwright show-report
```

## Coverage

E2E tests cover the following critical user flows:
- ✅ User authentication and authorization
- ✅ Dashboard account overview
- ✅ Transaction viewing and filtering
- ✅ Navigation between pages
- ✅ Error handling and recovery

## Future Enhancements

- Add transfer flow tests (T022)
- Add visual regression testing
- Add performance metrics
- Add mobile viewport tests
- Add accessibility (a11y) tests
