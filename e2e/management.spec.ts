import {test,expect,APIRequestContext} from '@playwright/test';
const base='http://127.0.0.1:5080',id='p29';
async function get(api:APIRequestContext){const r=await api.get(base+'/api/admin/products/'+id);expect(r.status()).toBe(200);return r.json();}
async function write(api:APIRequestContext,path:string,data:any){const token=(await(await api.get(base+'/api/auth/csrf')).json()).token;return api.post(base+path,{headers:{'X-CSRF-TOKEN':token},data});}
async function product(api:APIRequestContext,p:any,extra:any={}){return write(api,'/api/admin/products/'+id,{name:p.name,nameAr:p.nameAr,category:p.category,priceHalalas:p.priceHalalas,version:p.version,...extra});}
async function adjust(api:APIRequestContext,op:string,q:number,reason='Browser verification'){const p=await get(api);return write(api,'/api/admin/inventory/'+id+'/adjustments',{operation:op,quantity:q,reason,stockVersion:p.stockVersion,idempotencyKey:crypto.randomUUID()});}
let original:any;
test.beforeEach(async({page})=>{original=await get(page.request);});
test.afterEach(async({page})=>{
 // Expiry test clears only this browser's cookies; restore using the isolated fixture API context.
 const {request}=await import('@playwright/test');const api=await request.newContext({storageState:'/tmp/merchant-admin-state.json'});
 const p=await get(api);expect((await product(api,p,original?{name:original.name,nameAr:original.nameAr,category:original.category,priceHalalas:original.priceHalalas}:{})).ok()).toBeTruthy();expect((await adjust(api,'correction',original.stock,'Restore isolated fixture')).ok()).toBeTruthy();await api.dispose();
});
async function details(page:any,inventory=false){await page.goto('/Admin/'+(inventory?'Inventory':'Products')+'?search='+encodeURIComponent(original.sku));await page.locator('.product-detail-link:visible').first().click();await expect(page.getByRole('button',{name:'Edit Product',exact:true})).toBeVisible();}
test('product editor saves explicit changes and preserves filtered rows',async({page})=>{
 await details(page);await page.getByRole('button',{name:'Edit Product',exact:true}).click();await expect(page.getByLabel('Product name',{exact:true})).toHaveValue(original.name);await page.getByLabel('Product name',{exact:true}).fill('Browser edited product');await page.getByLabel('Selling price (SAR)',{exact:true}).fill('123.45');
 await page.screenshot({path:'test-results/'+test.info().project.name+'/product-editor.png'});
 await page.getByRole('button',{name:'Save Changes',exact:true}).click();await expect(page.getByRole('heading',{name:'Change recorded'})).toBeVisible();expect((await get(page.request)).priceHalalas).toBe(12345);await expect(page.locator('.product-detail-link:visible').first()).toContainText('Browser edited product');expect(new URL(page.url()).searchParams.get('search')).toBe(original.sku);
 await page.getByRole('button',{name:'View History',exact:true}).click();await expect(page.locator('.audit-list')).toContainText('product.updated');await expect(page.locator('.audit-list')).toContainText('Browser edited product');
});
test('invalid price and cancel never save; stale product offers refresh and review',async({page})=>{
 await details(page);await page.getByRole('button',{name:'Edit Product',exact:true}).click();await page.getByLabel('Selling price (SAR)',{exact:true}).fill('-1');await page.getByRole('button',{name:'Save Changes',exact:true}).click();expect(await page.getByLabel('Selling price (SAR)',{exact:true}).evaluate((el:HTMLInputElement)=>el.validity.valid)).toBeFalsy();expect((await get(page.request)).priceHalalas).toBe(original.priceHalalas);
 await page.getByRole('button',{name:'Cancel',exact:true}).click();await page.getByRole('button',{name:'Edit Product',exact:true}).click();await product(page.request,await get(page.request),{name:'Another administrator update'});await page.getByRole('button',{name:'Save Changes',exact:true}).click();await expect(page.getByRole('alert')).toContainText('record changed');await page.getByRole('button',{name:'Refresh and review'}).click();await expect(page.getByLabel('Product name',{exact:true})).toHaveValue('Another administrator update');
});
test('restock confirmation prevents double submission and reports authoritative result',async({page})=>{
 await details(page,true);await page.getByRole('button',{name:'Adjust Stock',exact:true}).click();await page.getByLabel('Quantity',{exact:true}).fill('3');await page.getByLabel('Reason',{exact:true}).fill('New delivery');await page.getByRole('button',{name:'Review Adjustment'}).click();await expect(page.locator('.review-summary')).toContainText('New delivery');
 await page.screenshot({path:'test-results/'+test.info().project.name+'/inventory-confirmation.png'});
 let writes=0;page.on('request',r=>{if(r.url().includes('/Management/AdjustInventory'))writes++;});await page.locator('.stock-form').evaluate((f:HTMLFormElement)=>{f.requestSubmit();f.requestSubmit();});await expect(page.getByRole('heading',{name:'Change recorded'})).toBeVisible();expect(writes).toBe(1);expect((await get(page.request)).stock).toBe(original.stock+3);await expect(page.getByRole('status').filter({hasText:'Stock changed from'})).toContainText('New delivery');
});
test('correction and damage retain reasons; stale stock rejects and history paginates',async({page})=>{
 for(const [op,q,reason] of [['correction',10,'Physical count'],['damage',2,'Damaged packaging']] as const){
 await details(page,true);await page.getByRole('button',{name:'Adjust Stock',exact:true}).click();await page.getByLabel('Adjustment type').selectOption(op);await page.getByLabel('Quantity',{exact:true}).fill(String(q));await page.getByLabel('Reason',{exact:true}).fill(reason);await page.getByRole('button',{name:'Review Adjustment'}).click();await page.getByRole('button',{name:'Confirm Adjustment',exact:true}).click();await expect(page.getByRole('heading',{name:'Change recorded'})).toBeVisible();}
 expect((await get(page.request)).stock).toBe(8);
 await details(page,true);await page.getByRole('button',{name:'Adjust Stock',exact:true}).click();await page.getByLabel('Adjustment type').selectOption('correction');await page.getByLabel('Quantity',{exact:true}).fill('50');await page.getByLabel('Reason',{exact:true}).fill('Stale physical count');await adjust(page.request,'restock',1);await page.getByRole('button',{name:'Review Adjustment'}).click();await page.getByRole('button',{name:'Confirm Adjustment',exact:true}).click();await expect(page.getByRole('alert')).toContainText('record changed');expect((await get(page.request)).stock).toBe(9);
 for(let i=0;i<6;i++)expect((await adjust(page.request,'restock',1,'History page '+i)).ok()).toBeTruthy();
 await details(page);await page.getByRole('button',{name:'View History',exact:true}).click();await expect(page.locator('.audit-entry')).toHaveCount(5);await page.screenshot({path:'test-results/'+test.info().project.name+'/audit-history.png'});await page.getByRole('button',{name:'Next history'}).click();await expect(page.getByRole('navigation',{name:'Audit pages'})).toContainText('Page 2');expect(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth)).toBeTruthy();
});
test('forms support keyboard cancel and reject expired session writes',async({page})=>{
 await details(page);await page.getByRole('button',{name:'Edit Product',exact:true}).click();await expect(page.locator('.management-title')).toBeFocused();await page.keyboard.press('Escape');await expect(page.locator('#product-drawer')).not.toHaveClass(/show/);
 await details(page);await page.getByRole('button',{name:'Edit Product',exact:true}).click();await page.context().clearCookies();await page.getByRole('button',{name:'Save Changes',exact:true}).click();await expect(page.getByRole('alert')).toContainText('Session expired');
});
test('uncertain stock response retries the same committed adjustment without duplication',async({page})=>{
 await details(page,true);await page.getByRole('button',{name:'Adjust Stock',exact:true}).click();await page.getByLabel('Quantity',{exact:true}).fill('1');await page.getByLabel('Reason',{exact:true}).fill('Retry verification');await page.getByRole('button',{name:'Review Adjustment'}).click();
 let first=true;await page.route('**/Management/AdjustInventory',async route=>{if(first){first=false;const response=await route.fetch();expect(response.status()).toBe(200);await route.abort('failed');}else await route.continue();});
 await page.getByRole('button',{name:'Confirm Adjustment',exact:true}).click();await expect(page.getByRole('alert')).toContainText('Retry sends the same request');await expect(page.getByLabel('Quantity',{exact:true})).toBeDisabled();expect((await get(page.request)).stock).toBe(original.stock+1);
 await page.getByRole('button',{name:'Confirm Adjustment',exact:true}).click();await expect(page.getByRole('heading',{name:'Change recorded'})).toBeVisible();expect((await get(page.request)).stock).toBe(original.stock+1);
});
