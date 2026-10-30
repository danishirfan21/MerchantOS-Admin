import { test, expect } from '@playwright/test';
const pages = [ ['overview','/','Overview'], ['products','/Admin/Products','Products'], ['inventory','/Admin/Inventory','Inventory'], ['orders','/Admin/Orders','Orders'], ['order-details','receipt','Order #'] ];
async function receipt(page: any) {
 const response = await page.request.get('http://127.0.0.1:5080/api/orders');
 const orders = await response.json();
 expect(orders.length).toBeGreaterThan(0);
 const detail = await (await page.request.get('http://127.0.0.1:5080/api/orders/' + orders[0].id)).json();
 return detail;
}
for (const [name,path,heading] of pages) {
 test(name + ' desktop page renders live POS data', async ({page},testInfo) => {
  const record = path === 'receipt' ? await receipt(page) : null;
  await page.goto(record ? '/Admin/Order/' + record.id : path);
  await expect(page.getByRole('heading',{level:1})).toContainText(heading);
  await expect(page.getByText('Unable to load POS data.')).toHaveCount(0);
  expect(await page.evaluate(()=>document.documentElement.scrollWidth <= innerWidth)).toBeTruthy();
  await expect(page.locator('#main')).toBeVisible();
  if(name==='overview') await expect(page.locator('.dashboard-metrics .metric').nth(2).locator('strong')).toHaveText('30');
  if(testInfo.project.name === 'desktop') await page.screenshot({path:'test-results/desktop/'+name+'.png',fullPage:false});
 });
}
test('sidebar navigation preserves active state', async ({page},testInfo) => {
 await page.goto('/');
 if(testInfo.project.name === 'mobile') await page.getByRole('button',{name:'Open navigation'}).click();
 await page.locator('#sidebar').getByRole('link',{name:'Inventory',exact:true}).click();
 await expect(page.getByRole('heading',{level:1,name:'Inventory'})).toBeVisible();
 await expect(page.locator('#sidebar a[aria-current=page]')).toHaveText('Inventory');
 if(testInfo.project.name==='desktop') {
  await page.getByLabel('Global product search').fill('Sony');
  await page.getByLabel('Global product search').press('Enter');
  await expect(page.getByRole('heading',{level:1,name:'Products'})).toBeVisible();
  await expect(page.locator('#product-rows strong').first()).toContainText('Sony');
 }
});
test('catalog search, categories and empty states', async ({page}) => {
 await page.goto('/Admin/Products');
 await page.getByLabel('Category',{exact:true}).selectOption('Audio');
 await expect(page.getByRole('status')).toContainText('products found');
 const categories = await page.locator('#product-rows .category-badge').allTextContents();
 expect(categories.length).toBeGreaterThan(0); expect(categories.every(c=>c==='Audio')).toBeTruthy();
 await page.getByLabel('Search products').fill('no-such-product');
 await expect(page.getByRole('heading',{name:'No products found'})).toBeVisible();
});
test('catalog pagination changes rows and preserves the query', async ({page}) => {
 await page.goto('/Admin/Products');
 const first=await page.locator('#product-rows strong').first().innerText();
 await expect(page.locator('#product-rows tr')).toHaveCount(7);
 await page.getByRole('link',{name:'Next',exact:true}).click();
 await expect(page.getByRole('status')).toHaveText('30 products found.');
 await expect(page).toHaveURL(/page=2/);
 expect(await page.locator('#product-rows strong').first().innerText()).not.toBe(first);
 await expect(page.locator('#result-pagination')).toContainText('8–14');
});
test('sorting compares numeric prices', async ({page}) => {
 await page.goto('/Admin/Products');
 await page.getByLabel('Sort products').selectOption('price-desc');
 await expect(page.getByRole('status')).toContainText('products found');
 const values=await page.locator('#product-rows td:nth-child(4)').allTextContents();
 const prices=values.map(v=>Number(v.replace(/[^0-9.]/g,'')));
 expect(prices).toEqual([...prices].sort((a,b)=>b-a));
});
test('product detail drawer loads an encoded read-only record', async ({page}) => {
 await page.goto('/Admin/Products');
 const name=await page.locator('#product-rows strong').first().innerText();
 await page.locator('#product-rows .product-detail-link').first().click();
 await expect(page.locator('#product-drawer .product-summary h2')).toHaveText(name);
 await expect(page.locator('#product-drawer')).toContainText('Available quantity');
 await expect(page.locator('#product-drawer input')).toHaveCount(0);
 await page.getByRole('button',{name:'Close product details'}).click();
 await expect(page.locator('#product-drawer')).not.toBeVisible();
});
test('inventory attention filter uses authoritative stock', async ({page}) => {
 await page.goto('/Admin/Inventory');
 await page.getByRole('link',{name:'Filter Low / Depleted'}).click();
 const quantities=await page.locator('#product-rows td:nth-child(4)').allTextContents();
 expect(quantities.length).toBeGreaterThan(0); expect(quantities.every(v=>Number(v)<=5)).toBeTruthy();
 await expect(page.getByLabel('Availability')).toHaveValue('attention');
 await page.getByLabel('Availability').selectOption('low');
 await expect(page.getByRole('status')).toContainText('products found');
 await expect(page.locator('#result-pagination a').first()).toHaveAttribute('href',/\/Admin\/Inventory/);
 await page.getByRole('link',{name:'Next',exact:true}).click();
 await expect(page).toHaveURL(/\/Admin\/Inventory.*page=2/);
});
test('orders browse persisted receipts and filter payment', async ({page}) => {
 await page.goto('/Admin/Orders');
 await page.getByLabel('Payment method',{exact:true}).selectOption('cash');
 await expect(page).toHaveURL(/payment=cash/);
 const methods=await page.locator('tbody .payment-label').allTextContents();
 expect(methods.length).toBeGreaterThan(0); expect(methods.every(v=>v.includes('Cash'))).toBeTruthy();
 await page.locator('tbody .row-action').first().click();
 await expect(page.getByRole('heading',{name:'Purchased Items',exact:false})).toBeVisible();
});
test('receipt preserves backend total and print action', async ({page}) => {
 const order=await receipt(page);
 await page.addInitScript(()=>{window.print=()=>{(window as any).__receiptPrinted=true;};});
 await page.goto('/Admin/Order/'+order.id);
 const total='SAR '+(order.payableHalalas/100).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});
 await expect(page.locator('.grand-total strong')).toHaveText(total);
 await page.getByRole('button',{name:'Print Receipt'}).click();
 expect(await page.evaluate(()=>(window as any).__receiptPrinted)).toBeTruthy();
 await page.emulateMedia({media:'print'});
 await expect(page.locator('.topbar')).not.toBeVisible();
 await expect(page.locator('.financial-summary')).toBeVisible();
 await expect(page.locator('.purchased-items')).toBeVisible();
 await page.pdf({path:'test-results/desktop/receipt.pdf',format:'A4',printBackground:true});
});
test('loading and failure states retain previous catalog rows', async ({page}) => {
 await page.goto('/Admin/Products');
 const first=await page.locator('#product-rows strong').first().innerText();
 await page.route('**/Admin/ProductRows?**',async route=>{await new Promise(resolve=>setTimeout(resolve,600));await route.fulfill({status:503,contentType:'application/json',body:'{"message":"Unavailable"}'});});
 await page.getByLabel('Search products').fill('test');
 await expect(page.getByRole('status')).toHaveText('Loading products…');
 await expect(page.getByRole('status')).toContainText('Unable to refresh products.');
 await expect(page.locator('#product-rows strong').first()).toHaveText(first);
});
test.describe('without JavaScript',()=>{
 test.use({javaScriptEnabled:false});
 test('Razor search and product details remain functional',async({page})=>{
  await page.goto('/Admin/Products');
  const name=await page.locator('#product-rows strong').first().innerText();
  await page.locator('#product-rows .product-detail-link').first().click();
  await expect(page.getByRole('heading',{level:1})).toHaveText(name);
  await page.goto('/Admin/Products');
  await page.getByLabel('Search products').fill('no-such-product');
  await page.getByRole('button',{name:'Apply filters'}).click();
  await expect(page.getByRole('heading',{name:'No products found'})).toBeVisible();
 });
});
