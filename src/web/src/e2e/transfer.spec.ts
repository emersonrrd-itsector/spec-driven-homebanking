import { test, expect } from '@playwright/test'
import { login, resetDatabase } from './helpers'

test.describe('Transfer Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Reset DB before each test for isolation
    await resetDatabase(page)
    // Login
    await login(page)
  })

  test('should execute a successful transfer between accounts', async ({ page }) => {
    // Navigate to Transfer page
    await page.getByRole('link', { name: /transfer/i }).click()
    await expect(page).toHaveURL('/transfer')
    
    // Select Source Account: Checking
    await page.getByLabel(/from account/i).selectOption({ index: 1 })

    // Select Destination Account: Savings
    await page.getByLabel(/to account/i).selectOption({ index: 2 })

    // Enter Amount
    await page.getByLabel(/amount/i).fill('100.00')

    // Enter Description
    await page.getByLabel(/description/i).fill('Test Transfer')

    // Submit Transfer
    await page.getByRole('button', { name: 'Transfer' }).click() // Exact match for button text

    // Wait for success toast or redirect to transactions
    // Assuming redirect to transactions based on typical flow
    await expect(page).toHaveURL(/\/transactions/)

    // Verify Balances on Dashboard
    await page.getByRole('link', { name: /dashboard/i }).click()
    await expect(page).toHaveURL('/dashboard')
    
    // Wait for balances to load/update
    // Reload page to be sure
    await page.reload()
    await page.waitForTimeout(1000)

    // Find card for Checking (Starts 5000 -> 4900)
    // Find the Card container that has "Checking" in it
    const checkingCard = page.locator('.bg-slate-900').filter({ hasText: 'Checking' }).first()
    await expect(checkingCard).toContainText('$4,900.00', { timeout: 10000 })

    // Find card for Savings (Starts 15000 -> 15100)
    const savingsCard = page.locator('.bg-slate-900').filter({ hasText: 'Savings' }).first()
    await expect(savingsCard).toContainText('$15,100.00', { timeout: 10000 })
  })

  test('should show error for insufficient funds', async ({ page }) => {
    await page.getByRole('link', { name: /transfer/i }).click()
    
    await page.getByLabel(/from account/i).selectOption({ index: 1 })
    await page.getByLabel(/to account/i).selectOption({ index: 2 })
    
    // Amount > Balance (5000)
    await page.getByLabel(/amount/i).fill('6000.00')
    await page.getByLabel(/amount/i).blur()
    
    // Trigger validation on other fields if needed (e.g. click body)
    await page.click('body')

    // Wait for button to be enabled?
    // If overdraft check is server-side, button should be ENABLED.
    // If overdraft check is client-side (not seen in code), button should be disabled.
    // Code doesn't check balance in existing validations.
    // So button MUST be enabled.
    
    await page.getByRole('button', { name: 'Transfer' }).click()
    
    // Expect error message
    await expect(page.getByText(/insufficient balance/i).first()).toBeVisible()
    
    // Ensure URL is still transfer
    expect(page.url()).toContain('/transfer')
  })

  test('should show validation error for negative amount', async ({ page }) => {
    await page.getByRole('link', { name: /transfer/i }).click()
    
    await page.getByLabel(/from account/i).selectOption({ index: 1 })
    await page.getByLabel(/to account/i).selectOption({ index: 2 })
    
    await page.getByLabel(/amount/i).fill('-50.00')
    await page.getByLabel(/amount/i).blur()

    // Verify validation error
    await expect(page.getByText(/Amount must be greater than/i)).toBeVisible()
    
    // Verify button disabled
    await expect(page.getByRole('button', { name: 'Transfer' })).toBeDisabled()
  })
})
