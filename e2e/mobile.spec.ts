import { test, expect } from '@playwright/test';
const pages=[['overview','/','Overview'],['products','/Admin/Products','Products'],['inventory','/Admin/Inventory','Inventory'],['orders','/Admin/Orders','Orders'],['order-details','receipt','Order #']];
async function orders(page:any){return (await page.request.get('http://127.0.0.1:5080/api/orders')).json();}
async function receiptUrl(page:any){return '/Admin/Order/'+(await orders(page))[0].id;}
for(const [name,url,heading] of pages){
 test(name+' mobile screen renders real data without horizontal overflow',async({page})=>{
  await page.goto(url==='receipt'?await receiptUrl(page):url);
  await expect(page.getByRole('heading',{level:1})).toContainText(heading);
  await expect(page.getByText('Unable to load POS data.')).toHaveCount(0);
  expect(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth)).toBeTruthy();
  await page.screenshot({path:'test-results/mobile/'+name+'.png'});
  await page.screenshot({path:'test-results/mobile/'+name+'-full.png',fullPage:true});
 });
}
test('bottom navigation visits all primary screens with the correct active state',async({page})=>{
 await page.goto('/');
 const nav=page.getByRole('navigation',{name:'Mobile navigation',exact:true});
 for(const title of ['Products','Inventory','Orders','Overview']){
  await nav.getByRole('link',{name:title,exact:true}).tap();
  await expect(page.getByRole('heading',{level:1,name:title})).toBeVisible();
  await expect(nav.locator('a[aria-current=page]>span:last-child')).toHaveText(title);
 }
 await page.screenshot({path:'test-results/mobile/navigation.png'});
 await page.getByRole('button',{name:'Open navigation',exact:true}).tap();
 await expect(page.locator('#sidebar')).toHaveClass(/show\b/);
 await page.screenshot({path:'test-results/mobile/navigation-drawer.png'});
 await page.keyboard.press('Escape');
 await expect(page.getByRole('button',{name:'Open navigation',exact:true})).toBeFocused();
});
test('product cards search, category, stock filters and numeric sorting stay synchronized',async({page})=>{
 await page.goto('/Admin/Products');
 const cards=page.locator('#product-cards .mobile-product-card');
 await expect(cards).toHaveCount(7);
 await expect(page.locator('.catalog-table')).not.toBeVisible();
 await page.getByLabel('Category',{exact:true}).selectOption('Audio');
 await expect(page.getByRole('status')).toContainText('products found');
 expect((await cards.locator('.mobile-card-topline>span:first-child').allTextContents()).every(t=>t==='Audio')).toBeTruthy();
 await page.getByLabel('Search products').fill('Sony');
 await expect(cards).toHaveCount(1);
 await expect(cards.first().locator('h2')).toContainText('Sony');
 await page.getByLabel('Search products').fill('no-such-product');
 await expect(page.getByRole('heading',{name:'No products found'})).toBeVisible();
 await page.getByRole('button',{name:'More filters & sorting',exact:true}).tap();
 await page.getByRole('link',{name:'Reset filters',exact:true}).click();
 await page.getByLabel('Availability',{exact:true}).selectOption('low');
 await expect(page.getByRole('status')).toContainText('products found');
 expect((await cards.locator('.mobile-available strong').allTextContents()).every(t=>Number(t.replace(/\D/g,''))<=5)).toBeTruthy();
 await page.getByRole('button',{name:'More filters & sorting',exact:true}).tap();
 await page.getByRole('link',{name:'Reset filters',exact:true}).click();
 await page.getByRole('button',{name:'More filters & sorting',exact:true}).tap();
 await page.getByLabel('Sort products').selectOption('price-asc');
 await expect(page.getByRole('status')).toContainText('products found');
 const prices=(await cards.locator('.mobile-price').allTextContents()).map(v=>Number(v.replace(/[^0-9.]/g,'')));
 expect(prices).toEqual([...prices].sort((a,b)=>a-b));
});
test('mobile catalog pagination updates cards and preserves query state',async({page})=>{
 await page.goto('/Admin/Products?sort=price-asc');
 const first=await page.locator('#product-cards h2').first().innerText();
 await page.getByRole('link',{name:'Next',exact:true}).tap();
 await expect(page).toHaveURL(/page=2/);
 await expect(page).toHaveURL(/sort=price-asc/);
 expect(await page.locator('#product-cards h2').first().innerText()).not.toBe(first);
 await expect(page.locator('#result-pagination')).toContainText('8–14');
});
test('mobile inventory attention, category and search use the actual stock',async({page})=>{
 await page.goto('/Admin/Inventory');
 await expect(page.locator('.catalog-metrics .metric')).toHaveCount(4);
 await expect(page.locator('.mobile-inventory-card')).toHaveCount(7);
 await page.getByRole('link',{name:'Filter Low / Depleted'}).tap();
 await expect(page).toHaveURL(/stock=attention/);
 await page.waitForLoadState('load');
 expect((await page.locator('.mobile-inventory-card .mobile-available strong').allTextContents()).every(t=>Number(t.replace(/\D/g,''))<=5)).toBeTruthy();
 await page.getByLabel('Category',{exact:true}).selectOption('Audio');
 await expect(page.getByRole('status')).toContainText('products found');
 await page.getByLabel('Search products').fill('Sony');
 await expect(page.locator('.mobile-inventory-card')).toHaveCount(1);
 await expect(page.locator('.mobile-inventory-card')).toContainText('Demo threshold: 5 units');
});
test('full-width product drawer supports touch, Escape, restored focus and scroll locking',async({page})=>{
 await page.goto('/Admin/Products');
 const link=page.locator('#product-cards .mobile-detail').first();
 await link.tap();
 const drawer=page.locator('#product-drawer');
 await expect(drawer).toBeVisible();
 await expect(drawer.locator('.product-summary h2')).toBeVisible();
 expect((await drawer.boundingBox())!.width).toBeCloseTo(390,2);
 await expect(page.locator('body')).toHaveCSS('overflow','hidden');
 await expect(drawer).toHaveClass(/show\b/);
 await page.getByRole('button',{name:'Close product details'}).focus();
 await page.keyboard.press('Escape');
 await expect(drawer).not.toBeVisible();
 await expect(link).toBeFocused();
 await link.tap();
 await page.getByRole('button',{name:'Close product details'}).tap();
 await expect(link).toBeFocused();
 await expect(page.locator('body')).not.toHaveCSS('overflow','hidden');
});
test('order cards search, filter, paginate and open the persisted receipt',async({page})=>{
 const records=await orders(page);expect(records.length).toBeGreaterThan(7);
 await page.goto('/Admin/Orders');
 await expect(page.locator('.mobile-order-card')).toHaveCount(7);
 await page.getByRole('link',{name:'Next',exact:true}).tap();
 await expect(page).toHaveURL(/page=2/);
 await page.getByLabel('Order number',{exact:true}).fill(records[0].orderNumber);
 await page.getByRole('button',{name:'Search',exact:true}).tap();
 await expect(page.locator('.mobile-order-card')).toHaveCount(1);
 await page.getByRole('link',{name:'Reset filters',exact:true}).tap();
 await page.getByLabel('Payment method',{exact:true}).selectOption('cash');
 await expect(page).toHaveURL(/payment=cash/);
 expect((await page.locator('.mobile-order-meta>span').allTextContents()).every(t=>t==='Cash')).toBeTruthy();
 await page.getByLabel('Date range (UTC)',{exact:true}).selectOption('week');
 await expect(page).toHaveURL(/range=week/);
 await page.locator('.mobile-order-card .btn').first().tap();
 await expect(page.getByRole('heading',{name:'Purchased Items',exact:false})).toBeVisible();
 await expect(page.getByRole('navigation',{name:'Mobile navigation',exact:true})).toHaveCount(0);
 await page.getByRole('link',{name:'Back to Orders',exact:true}).tap();
 await expect(page.getByRole('heading',{name:'Orders',exact:true})).toBeVisible();
});
test('mobile receipt preserves backend values, section order and a single print action',async({page})=>{
 const record=(await orders(page))[0];
 const order=await (await page.request.get('http://127.0.0.1:5080/api/orders/'+record.id)).json();
 await page.addInitScript(()=>{window.print=()=>{(window as any).__printed=true;};});
 await page.goto('/Admin/Order/'+order.id);
 await expect(page.locator('.grand-total strong')).toHaveText('SAR '+(order.payableHalalas/100).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2}));
 await expect(page.getByRole('button',{name:'Print Receipt'})).toHaveCount(1);
 const rects=await page.locator('.purchased-items,.summary-content,.payment-content').evaluateAll(es=>es.map(e=>({top:e.getBoundingClientRect().top,bottom:e.getBoundingClientRect().bottom})));
 expect(rects[1].top).toBeGreaterThan(rects[0].bottom);expect(rects[2].top).toBeGreaterThan(rects[1].bottom);
 await page.getByRole('button',{name:'Print Receipt'}).tap();
 expect(await page.evaluate(()=>(window as any).__printed)).toBeTruthy();
 await page.emulateMedia({media:'print'});
 await expect(page.locator('.mobile-receipt-bar')).not.toBeVisible();
 await expect(page.locator('.financial-summary')).toBeVisible();
 await page.pdf({path:'test-results/mobile/receipt.pdf',format:'A4',printBackground:true});
});
test('controls fit without overlap and bottom navigation leaves final actions reachable at 390 and 360',async({page})=>{
 for(const width of [390,360]){
  await page.setViewportSize({width,height:844});
  for(const url of ['/', '/Admin/Products','/Admin/Inventory','/Admin/Orders',await receiptUrl(page)]){
   await page.goto(url);
   expect(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth)).toBeTruthy();
   const bad=await page.locator('.filter-bar input,.filter-bar select,.filter-bar button,.filter-bar a,.mobile-bottom-nav a').evaluateAll(es=>es.filter(e=>{const r=e.getBoundingClientRect();return r.width>0&&(r.left<0||r.right>innerWidth||r.height<43);}).map(e=>e.outerHTML));expect(bad).toEqual([]);
   const overlap=await page.locator('.filter-bar input,.filter-bar select,.filter-bar button,.filter-bar a').evaluateAll(es=>{const rs=es.map(e=>e.getBoundingClientRect()).filter(r=>r.width>0);return rs.some((a,i)=>rs.slice(i+1).some(b=>Math.min(a.right,b.right)>Math.max(a.left,b.left)+1&&Math.min(a.bottom,b.bottom)>Math.max(a.top,b.top)+1));});expect(overlap).toBeFalsy();
   await page.evaluate(()=>window.scrollTo({top:document.documentElement.scrollHeight,behavior:'instant'}));
   if(!url.includes('/Order/')){const nav=await page.locator('.mobile-bottom-nav').boundingBox();const main=await page.locator('#main').boundingBox();expect(main!.y+main!.height-90).toBeLessThanOrEqual(nav!.y+2);}
   await expect(page.locator('#main')).toBeVisible();
  }
 }
});
test('mobile loading and failure states retain cards',async({page})=>{
 await page.goto('/Admin/Products');const first=await page.locator('#product-cards h2').first().innerText();
 await page.route('**/Admin/ProductRows?**',async route=>{await new Promise(r=>setTimeout(r,600));await route.fulfill({status:503,contentType:'application/json',body:'{"message":"Unavailable"}'});});
 await page.getByLabel('Search products').fill('test');
 await expect(page.getByRole('status')).toHaveText('Loading products…');
 await expect(page.locator('#product-cards')).toHaveAttribute('aria-busy','true');
 await expect(page.getByRole('status')).toContainText('Unable to refresh products.');
 await expect(page.locator('#product-cards h2').first()).toHaveText(first);
});
test.describe('without JavaScript',()=>{test.use({javaScriptEnabled:false});test('mobile navigation, GET search and product pages remain functional',async({page})=>{
 await page.goto('/');await page.getByRole('navigation',{name:'Mobile navigation'}).getByRole('link',{name:'Products',exact:true}).click();
 const name=await page.locator('#product-cards h2').first().innerText();await page.locator('#product-cards h2 a').first().click();
 await expect(page.getByRole('heading',{level:1})).toHaveText(name);
 await page.goto('/Admin/Products');await page.getByLabel('Search products').fill('no-such-product');await page.getByRole('button',{name:'Apply filters'}).click();
 await expect(page.getByRole('heading',{name:'No products found'})).toBeVisible();
});});
