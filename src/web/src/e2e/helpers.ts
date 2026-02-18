import { Page, expect } from '@playwright/test'

/**
 * Test helpers and utilities for E2E tests
 * Common functions to reduce code duplication across test files
 */

/**
 * Login helper - performs login flow and waits for dashboard
 * @param page Playwright page object
 * @param email User email (default: demo credentials)
 * @param password User password (default: demo credentials)
 */
export async function login(
  page: Page,
  email: string = 'admin@homebank.local',
  password: string = 'demo123'
) {
  await page.goto('/login')
  await page.evaluate(() => localStorage.clear())
  
  await page.getByLabel(/email/i).fill(email)
  await page.getByLabel(/password/i).fill(password)
  
  // Click login button
  await page.getByRole('button', { name: /login/i }).click()

  // Wait for token to be set in localStorage
  await page.waitForFunction(() => !!localStorage.getItem('homebank_jwt'), null, { timeout: 10000 })
  
  // Wait for navigation to dashboard
  await page.waitForURL('**/dashboard', { timeout: 10000 })
}

/**
 * Navigate to transactions page for first available account
 * Assumes user is already logged in and on dashboard
 */
export async function navigateToTransactions(page: Page) {
  // Wait for account cards to load
  await page.waitForSelector('[class*="animate-pulse"]', { state: 'detached', timeout: 10000 })
  
  // Click first account card
  const firstCard = page.locator('[class*="bg-slate-900"][class*="border-slate-700"]').filter({
    has: page.locator('text=/Balance|Account #/i')
  }).first()
  
  await firstCard.click()
  
  // Wait for transactions page
  await expect(page).toHaveURL(/\/transactions\?accountId=/, { timeout: 5000 })
}

/**
 * Wait for transactions table to load (no loading state)
 */
export async function waitForTransactionsTable(page: Page) {
  await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
}

/**
 * Get all transaction rows from the table
 */
export function getTransactionRows(page: Page) {
  return page.getByRole('row').filter({
    has: page.getByRole('cell')
  })
}

/**
 * Get account cards from dashboard
 */
export function getAccountCards(page: Page) {
  return page.locator('[class*="bg-slate-900"][class*="border-slate-700"]').filter({
    has: page.locator('text=/Balance|Account #/i')
  })
}

/**
 * Clear authentication and navigate to login
 */
export async function logout(page: Page) {
  await page.evaluate(() => localStorage.clear())
  await page.goto('/login')
}

/**
 * Check if JWT token is stored in localStorage
 */
export async function hasValidJwtToken(page: Page): Promise<boolean> {
  const token = await page.evaluate(() => localStorage.getItem('homebank_jwt'))
  return token !== null && token.split('.').length === 3
}

/**
 * Format expected currency string for assertions
 */
export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

/**
 * Valid transaction categories
 */
export const VALID_CATEGORIES = [
  'Groceries',
  'Utilities',
  'Entertainment',
  'Food & Dining',
  'Transport',
  'Salary',
  'Transfer',
  'Other',
] as const

/**
 * Check if a string is a valid transaction category
 */
export function isValidCategory(category: string): boolean {
  return VALID_CATEGORIES.includes(category as typeof VALID_CATEGORIES[number])
}

/**
 * Reset database to initial state for testing
 * Uses the API test endpoint to clear and seed data
 */
export async function resetDatabase(page: Page) {
  const response = await page.request.post('http://localhost:5087/api/test/reset')
  expect(response.ok()).toBeTruthy()
}
