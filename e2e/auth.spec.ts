import {test,expect} from '@playwright/test';
import {completeMfa,mvcLogin} from './mfa.fixture';
test.use({storageState:{cookies:[],origins:[]},trace:'off',screenshot:'off'});
async function fillPassword(page:any,value:string){await page.locator('#Password').evaluate((el:HTMLInputElement,password:string)=>{el.value=password;},value);}
test('anonymous routes redirect safely and JSON rejects direct access',async({page})=>{
 const response=await page.request.get('/Admin/ProductRows',{maxRedirects:0});expect(response.status()).toBe(401);
 await page.goto('/Admin/Inventory?stock=low');await expect(page).toHaveURL(/Account\/Login/);
 await expect(page.getByLabel('Username',{exact:true})).toBeVisible();
 await page.screenshot({path:'test-results/'+test.info().project.name+'/login.png'});
 expect(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth)).toBeTruthy();
});
test('administrator signs in, returns to the query, and logout revokes the session',async({page})=>{
 await page.goto('/Admin/Inventory?stock=low');await page.getByLabel('Username',{exact:true}).fill(process.env['TEST_ADMIN_USERNAME']!);await fillPassword(page,process.env['TEST_ADMIN_PASSWORD']!);
 await page.getByRole('button',{name:'Sign In',exact:true}).click();await completeMfa(page);await expect(page).toHaveURL(/Admin\/Inventory\?stock=low/);
 const cookies=await page.context().cookies();const session=cookies.find(c=>c.name==='MerchantOS.Admin.Session');expect(session?.httpOnly).toBeTruthy();expect(session?.sameSite).toBe('Strict');expect(cookies.some(c=>c.name==='MerchantOS.Pos.Session')).toBeFalsy();
 if(test.info().project.name!=='desktop')await page.getByRole('button',{name:'Open navigation',exact:true}).click();
 await page.getByRole('button',{name:'Sign out',exact:true}).click();await expect(page).toHaveURL(/Account\/Login/);
 expect((await page.request.get('/Admin/ProductRows',{maxRedirects:0})).status()).toBe(401);
});
test('cashier cannot sign in to Admin and external returns never redirect off-site',async({page})=>{
 await page.goto('/Account/Login?returnUrl=https://example.com');await page.getByLabel('Username',{exact:true}).fill(process.env['TEST_CASHIER_USERNAME']!);await fillPassword(page,process.env['TEST_CASHIER_PASSWORD']!);
 await page.getByRole('button',{name:'Sign In',exact:true}).click();await expect(page.getByRole('alert')).toContainText('Unable to sign in');
 await page.getByLabel('Username',{exact:true}).fill(process.env['TEST_ADMIN_USERNAME']!);await fillPassword(page,process.env['TEST_ADMIN_PASSWORD']!);await page.getByRole('button',{name:'Sign In',exact:true}).click();
 await completeMfa(page);await expect(page).toHaveURL('http://127.0.0.1:5090/');
});
test('forged MVC login and logout requests fail CSRF validation',async({page})=>{
 expect((await page.request.post('/Account/Login',{form:{Username:'missing',Password:'invalid'}})).status()).toBe(400);
 await mvcLogin(page.request);
 expect((await page.request.post('/Account/Logout',{form:{}})).status()).toBe(400);
});
