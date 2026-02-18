# Validation Report for T022: E2E Test Fixes and Improvements

## Status: COMPLETED

### 1. Test Failures & Corrections

The following E2E tests have been identified and fixed to address flakiness and synchronization issues.

#### src/web/src/e2e/login.spec.ts
- **Issue**: `should validate email format` test was using `.blur()` to trigger validation, which is invalid for form submit validation logic.
- **Fix**: Replaced `.blur()` with `.click()` on the submit button. This ensures the form validation runs correctly.
- **Issue**: `should show error message with invalid credentials` test was asserting URL immediately after click, leading to race conditions where the API response (401) wasn't processed yet.
- **Fix**: Added `await page.waitForResponse` to ensure the 401 response is captured before asserting the error message.

#### src/web/src/e2e/transactions.spec.ts
- **Issue**: `should filter transactions by category` and related filter tests were using `page.locator('select').first()`. This selector was ambiguous and mistakenly selected the "Account" dropdown instead of the "Category" filter.
- **Fix**: Updated the selector to use the specific ID `page.locator('#category-select')`.
- **Affected Tests**:
  - `should filter transactions by category`
  - `should show all transactions when filter is cleared`
  - `should maintain filter when changing pages`

### 2. Execution Environment
- **API Server**: Running on `http://localhost:5087` via `dotnet run`.
- **Frontend Server**: Running on `http://localhost:5173` via `vite`.
- **Test Runner**: Playwright (v1.40+).

### 3. Conclusion
The E2E tests have been updated to align with the current implementation of the application. The fixes address synchronization issues in the login flow and selector specificity issues in the transaction filtering tests. These changes improve test stability and accuracy.
