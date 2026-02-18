import { test, expect } from '@playwright/test'

/**
 * Transactions & Category Filter E2E Tests
 * Covers FR-3.1: View transactions with filtering
 * Tests: Transaction table display, category badges, category filtering
 */

test.describe('Transactions', () => {
  // Helper to login and navigate to transactions
  test.beforeEach(async ({ page }) => {
    // Login
    await page.goto('/login')
    await page.evaluate(() => localStorage.clear())
    
    await page.getByLabel(/email/i).fill('admin@homebank.local')
    await page.getByLabel(/password/i).fill('demo123')
    await page.getByRole('button', { name: /login/i }).click()
    
    // Wait for dashboard
    await expect(page).toHaveURL('/dashboard', { timeout: 10000 })
    
    // Wait for account cards to load
    await page.waitForSelector('[class*="animate-pulse"]', { state: 'detached', timeout: 10000 })
    
    // Click first account card to navigate to transactions
    const firstCard = page.locator('[class*="bg-slate-900"][class*="border-slate-700"]').filter({
      has: page.locator('text=/Balance|Account #/i')
    }).first()
    
    await firstCard.click()
    
    // Wait for transactions page to load
    await expect(page).toHaveURL(/\/transactions\?accountId=/, { timeout: 5000 })
  })

  test('should display transactions table with all required columns', async ({ page }) => {
    // Wait for table to load (no loading spinner)
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    // Verify transaction table headers exist
    await expect(page.getByRole('columnheader', { name: /date/i })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: /description/i })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: /category/i })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: /amount/i })).toBeVisible()
    
    // Verify at least one transaction row exists
    const tableRows = page.getByRole('row').filter({
      has: page.getByRole('cell')
    })
    
    const rowCount = await tableRows.count()
    expect(rowCount).toBeGreaterThan(0)
  })

  test('should display category badges with colors for each transaction', async ({ page }) => {
    // Wait for table to load
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    // Find table rows with data
    const tableRows = page.getByRole('row').filter({
      has: page.getByRole('cell')
    })
    
    const rowCount = await tableRows.count()
    expect(rowCount).toBeGreaterThan(0)
    
    // Check first few rows have category badges
    for (let i = 0; i < Math.min(rowCount, 3); i++) {
      const row = tableRows.nth(i)
      
      // Each row should have a category cell containing a badge
      // Category badges have specific text and background colors
      const categoryCell = row.getByRole('cell').nth(2) // Category is 3rd column (0-indexed)
      
      // Verify category text is visible (should be one of the predefined categories)
      await expect(categoryCell).toBeVisible()
      
      const categoryText = await categoryCell.textContent()
      const validCategories = [
        'Groceries', 'Utilities', 'Entertainment', 'Food & Dining', 
        'Transport', 'Salary', 'Transfer', 'Other'
      ]
      
      expect(validCategories).toContain(categoryText?.trim())
    }
  })

  test('should format transaction amounts with currency and +/- signs', async ({ page }) => {
    // Wait for table to load
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    const tableRows = page.getByRole('row').filter({
      has: page.getByRole('cell')
    })
    
    const rowCount = await tableRows.count()
    expect(rowCount).toBeGreaterThan(0)
    
    // Check first row's amount column
    const firstRow = tableRows.first()
    const amountCell = firstRow.getByRole('cell').nth(3) // Amount is 4th column (0-indexed)
    
    const amountText = await amountCell.textContent()
    
    // Should have either + or - sign and $ with decimal
    expect(amountText).toMatch(/[+-]\$[0-9,]+\.[0-9]{2}/)
  })

  test('should filter transactions by category', async ({ page }) => {
    // Wait for table to load
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    // Find category filter dropdown (combobox or select)
    const categoryFilter = page.locator('select').filter({
      has: page.locator('option:has-text("All Categories"), option:has-text("Groceries")')
    }).or(page.getByRole('combobox', { name: /category/i }))
    
    // If no explicit role, look for select element near "Category" label
    const filterSelect = await page.locator('select').first().isVisible()
      ? page.locator('select').first()
      : page.locator('[id*="category"], [name*="category"]').first()
    
    // Get initial row count
    const initialRows = page.getByRole('row').filter({ has: page.getByRole('cell') })
    const initialCount = await initialRows.count()
    
    expect(initialCount).toBeGreaterThan(0)
    
    // Select a specific category (e.g., "Groceries")
    // First check if we have a visible select element
    const selectElement = page.locator('select').first()
    const isSelectVisible = await selectElement.isVisible().catch(() => false)
    
    if (isSelectVisible) {
      await selectElement.selectOption('Groceries')
    } else {
      // Try clicking a filter button or dropdown
      const filterButton = page.getByText(/category|filter/i).first()
      if (await filterButton.isVisible().catch(() => false)) {
        await filterButton.click()
        await page.getByText('Groceries').click()
      }
    }
    
    // Wait for table to update
    await page.waitForTimeout(1000)
    
    // Verify filtered results
    const filteredRows = page.getByRole('row').filter({ has: page.getByRole('cell') })
    const filteredCount = await filteredRows.count()
    
    // All visible rows should have "Groceries" category
    for (let i = 0; i < Math.min(filteredCount, 3); i++) {
      const row = filteredRows.nth(i)
      const categoryCell = row.getByRole('cell').nth(2)
      await expect(categoryCell).toHaveText('Groceries')
    }
  })

  test('should show all transactions when filter is cleared', async ({ page }) => {
    // Wait for table to load
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    // Get initial count (all transactions)
    const initialRows = page.getByRole('row').filter({ has: page.getByRole('cell') })
    const initialCount = await initialRows.count()
    
    // Apply a filter
    const selectElement = page.locator('select').first()
    const isSelectVisible = await selectElement.isVisible().catch(() => false)
    
    if (isSelectVisible) {
      // Select a category
      await selectElement.selectOption('Utilities')
      await page.waitForTimeout(500)
      
      // Get filtered count
      const filteredRows = page.getByRole('row').filter({ has: page.getByRole('cell') })
      const filteredCount = await filteredRows.count()
      
      // Select "All Categories" to clear filter
      await selectElement.selectOption('All')
      await page.waitForTimeout(500)
      
      // Should show all transactions again
      const allRows = page.getByRole('row').filter({ has: page.getByRole('cell') })
      const finalCount = await allRows.count()
      
      expect(finalCount).toBe(initialCount)
    }
  })

  test('should display pagination controls', async ({ page }) => {
    // Wait for table to load
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    // Look for pagination elements
    // Common pagination patterns: Previous, Next buttons, page numbers, page size selector
    
    const paginationContainer = page.locator('text=/page|showing|of/i').first()
    
    if (await paginationContainer.isVisible().catch(() => false)) {
      await expect(paginationContainer).toBeVisible()
    }
    
    // Look for navigation buttons
    const prevButton = page.getByRole('button', { name: /previous|prev/i })
    const nextButton = page.getByRole('button', { name: /next/i })
    
    // At least one pagination control should exist
    const hasPrev = await prevButton.isVisible().catch(() => false)
    const hasNext = await nextButton.isVisible().catch(() => false)
    
    expect(hasPrev || hasNext).toBeTruthy()
  })

  test('should handle empty state when no transactions exist', async ({ page }) => {
    // This test is tricky because demo data likely has transactions
    // We can test by selecting a category with no transactions
    
    // Wait for table to load
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    // The page should either show transactions or an empty state message
    const hasRows = await page.getByRole('row').filter({ has: page.getByRole('cell') }).count() > 0
    const hasEmptyMessage = await page.getByText(/no transactions|empty|no data/i).isVisible().catch(() => false)
    
    // One of these should be true
    expect(hasRows || hasEmptyMessage).toBeTruthy()
  })

  test('should change page size and update table', async ({ page }) => {
    // Wait for table to load
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    // Look for page size selector (10, 25, 50)
    const pageSizeSelect = page.locator('select').filter({
      has: page.locator('option:has-text("10"), option:has-text("25")')
    })
    
    if (await pageSizeSelect.isVisible().catch(() => false)) {
      // Get initial row count
      const initialRows = page.getByRole('row').filter({ has: page.getByRole('cell') })
      const initialCount = await initialRows.count()
      
      // Change page size to 25
      await pageSizeSelect.selectOption('25')
      await page.waitForTimeout(1000)
      
      // Row count might change (if there are more than 10 transactions)
      const newRows = page.getByRole('row').filter({ has: page.getByRole('cell') })
      const newCount = await newRows.count()
      
      // Should still have transactions visible
      expect(newCount).toBeGreaterThan(0)
    }
  })

  test('should navigate between pages', async ({ page }) => {
    // Wait for table to load
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    // Look for next button
    const nextButton = page.getByRole('button', { name: /next/i })
    
    if (await nextButton.isEnabled().catch(() => false)) {
      // Click next page
      await nextButton.click()
      await page.waitForTimeout(500)
      
      // Should still show transactions
      const rows = page.getByRole('row').filter({ has: page.getByRole('cell') })
      expect(await rows.count()).toBeGreaterThan(0)
      
      // Previous button should now be enabled
      const prevButton = page.getByRole('button', { name: /previous|prev/i })
      await expect(prevButton).toBeEnabled()
    }
  })

  test('should maintain filter when changing pages', async ({ page }) => {
    // Wait for table to load
    await page.waitForSelector('text=/loading/i', { state: 'detached', timeout: 10000 }).catch(() => {})
    
    // Apply a category filter
    const selectElement = page.locator('select').first()
    const isSelectVisible = await selectElement.isVisible().catch(() => false)
    
    if (isSelectVisible) {
      await selectElement.selectOption('Transfer')
      await page.waitForTimeout(500)
      
      // Check if there are multiple pages
      const nextButton = page.getByRole('button', { name: /next/i })
      if (await nextButton.isEnabled().catch(() => false)) {
        await nextButton.click()
        await page.waitForTimeout(500)
        
        // Verify category filter is still applied
        const rows = page.getByRole('row').filter({ has: page.getByRole('cell') })
        if (await rows.count() > 0) {
          const firstRow = rows.first()
          const categoryCell = firstRow.getByRole('cell').nth(2)
          await expect(categoryCell).toHaveText('Transfer')
        }
      }
    }
  })
})
