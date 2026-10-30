import { test, expect } from '@playwright/test';
test.beforeEach(async ({},info)=>{test.skip(info.project.name!=='tablet','Approved tablet viewport only');});
test('drawer supports touch, Escape, focus trapping and focus restoration',async({page})=>{
 await page.goto('/');
 const trigger=page.getByRole('button',{name:'Open navigation',exact:true});
 await expect(page.locator('#sidebar')).not.toBeVisible();
 await trigger.tap();
 await expect(trigger).toHaveAttribute('aria-expanded','true');
 await expect(page.getByRole('dialog',{name:'MerchantOS'})).toBeVisible();
 await page.getByRole('button',{name:'Close navigation',exact:true}).focus();
 await page.keyboard.press('Tab');
 expect(await page.locator('#sidebar').evaluate(el=>el.contains(document.activeElement))).toBeTruthy();
 await page.keyboard.press('Escape');
 await expect(page.locator('#sidebar')).not.toBeVisible();
 await expect(trigger).toBeFocused();
 await expect(trigger).toHaveAttribute('aria-expanded','false');
 await trigger.click();
 await page.getByRole('button',{name:'Close navigation',exact:true}).click();
 await expect(trigger).toBeFocused();
});
test('tablet layouts stack sections and simplify the tables',async({page})=>{
 await page.goto('/');
 const cards=await page.locator('.dashboard-metrics .metric').evaluateAll(es=>es.map(e=>({x:e.getBoundingClientRect().x,y:e.getBoundingClientRect().y})));
 expect(cards[0].y).toBe(cards[1].y);expect(cards[2].y).toBeGreaterThan(cards[0].y);
 const recent=await page.locator('.recent-orders').boundingBox(),stock=await page.locator('.stock-watch').boundingBox();
 expect(stock!.y).toBeGreaterThanOrEqual(recent!.y+recent!.height);
 await page.goto('/Admin/Products');
 await expect(page.locator('thead th:visible')).toHaveCount(4);
 await expect(page.locator('#product-rows .product-name-link').first()).toBeVisible();
 const name=page.locator('#product-rows .product-name-link').first();
 await name.focus();await page.keyboard.press('Enter');
 await expect(page.locator('#product-drawer')).toBeVisible();
 await page.keyboard.press('Escape');await expect(name).toBeFocused();
 await page.goto('/Admin/Orders');
 await expect(page.locator('thead th:visible')).toHaveCount(4);
});
test('tablet order search, UTC date filter and pagination use the returned window',async({page})=>{
 const orders=await (await page.request.get('http://127.0.0.1:5080/api/orders')).json();
 await page.goto('/Admin/Orders');
 await page.getByLabel('Order number',{exact:true}).fill(orders[0].orderNumber);
 await page.getByRole('button',{name:'Search',exact:true}).click();
 await expect(page.locator('tbody tr')).toHaveCount(1);
 await page.getByRole('link',{name:'Reset filters',exact:true}).click();
 if(orders.length>7){
  const first=await page.locator('tbody tr').first().innerText();
  await page.getByRole('link',{name:'Next',exact:true}).click();
  await expect(page).toHaveURL(/page=2/);
  expect(await page.locator('tbody tr').first().innerText()).not.toBe(first);
 }
 await page.getByLabel('Date range (UTC)',{exact:true}).selectOption('week');
 await expect(page).toHaveURL(/range=week/);
 await expect(page.locator('.data-scope')).toContainText('latest 100');
});
test('tablet forms and tables fit and receipt has one final print action',async({page})=>{
 for(const url of ['/Admin/Products','/Admin/Inventory','/Admin/Orders']){
  await page.goto(url);
  const problems=await page.locator('.filter-bar input,.filter-bar select,.filter-bar button,.filter-bar a').evaluateAll(es=>es.filter(e=>{const r=e.getBoundingClientRect();return r.width>0&&(r.left<0||r.right>innerWidth||r.height<40);}).map(e=>e.outerHTML));
  expect(problems).toEqual([]);
  expect(await page.locator('.table-responsive').evaluateAll(es=>es.every(e=>e.scrollWidth<=e.clientWidth))).toBeTruthy();
 }
 const orders=await (await page.request.get('http://127.0.0.1:5080/api/orders')).json();
 await page.goto('/Admin/Order/'+orders[0].id);
 await expect(page.getByRole('button',{name:'Print Receipt'})).toHaveCount(1);
 const positions=await page.locator('.back-link,.order-heading>div,.purchased-items,.summary-content,.payment-content,.print-receipt').evaluateAll(es=>es.map(e=>e.getBoundingClientRect().y));
 // DOM places print in the header; tablet grid puts the same action after payment.
 expect(positions[2]).toBeGreaterThan(positions[5]);
 expect(await page.locator('.print-receipt').evaluate(e=>e.getBoundingClientRect().top)).toBeGreaterThan(await page.locator('.payment-content').evaluate(e=>e.getBoundingClientRect().bottom));
});
