import { test, expect } from '@playwright/test';
test('overview renders supported live metrics', async ({page}) => {
 await page.goto('/');
 await expect(page.getByRole('heading',{name:'Operations overview'})).toBeVisible();
 await expect(page.getByText('Orders and sales cover the latest 100 orders, across all dates.')).toBeVisible();
 await expect(page.locator('.metric').first().locator('strong')).toHaveText('30');
 await expect(page.getByText('Unable to load POS data.')).toHaveCount(0);
});
test('catalog filtering, empty state and product details', async ({page}) => {
 await page.goto('/Admin/Products');
 const firstName = await page.locator('#product-rows tr').first().locator('strong').innerText();
 await page.getByLabel('Search products').fill(firstName);
 await expect(page.getByRole('status')).toContainText('products found');
 await expect(page.locator('#product-rows tr').first().locator('strong')).toHaveText(firstName);
 await page.locator('#product-rows a').first().click();
 await expect(page.getByRole('heading',{level:1})).toHaveText(firstName);
 await page.goto('/Admin/Products?search=no-such-product');
 await expect(page.getByRole('heading',{name:'No products found'})).toBeVisible();
});
test('inventory stock filter and real order receipt', async ({page}) => {
 await page.goto('/Admin/Inventory');
 await page.getByLabel('Availability').selectOption('out');
 await expect(page.getByRole('status')).toContainText('products found');
 const statuses=await page.locator('#product-rows .stock-badge').allTextContents();
 expect(statuses.every(s=>s==='Out of stock')).toBeTruthy();
 await page.goto('/Admin/Orders');
 await page.locator('tbody a').first().click();
 await expect(page.getByRole('heading',{name:'Order items'})).toBeVisible();
 await expect(page.getByText('Total paid')).toBeVisible();
});
test('navigation and responsive layout', async ({page},testInfo) => {
 await page.goto('/');
 if(testInfo.project.name==='mobile') {
  await page.getByRole('button',{name:'Open navigation'}).click();
  await expect(page.locator('#sidebar')).toBeVisible();
 }
 await page.getByRole('link',{name:'Inventory',exact:true}).click();
 await expect(page.getByRole('heading',{name:'Inventory overview'})).toBeVisible();
 expect(await page.evaluate(()=>document.documentElement.scrollWidth <= innerWidth)).toBeTruthy();
 await page.screenshot({path:`test-results/${testInfo.project.name}-inventory.png`,fullPage:true});
});
test.describe('without JavaScript', () => {
 test.use({javaScriptEnabled:false});
 test('Razor search works without JavaScript', async ({page}) => {
  await page.goto('/Admin/Products');
  await page.getByLabel('Search products').fill('no-such-product');
  await page.getByRole('button',{name:'Apply filters'}).click();
  await expect(page.getByRole('heading',{name:'No products found'})).toBeVisible();
 });
});
