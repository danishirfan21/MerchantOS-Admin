import {test,expect,Page} from '@playwright/test';
import {execFileSync} from 'node:child_process';
import {randomBytes} from 'node:crypto';
import {mkdtempSync,writeFileSync,readFileSync,chmodSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {resolve,join} from 'node:path';
test.use({storageState:{cookies:[],origins:[]},trace:'off',screenshot:'off'});
const project=resolve('reference-pos/services/merchant-api/MerchantOS.Api.csproj');
function sql(statement:string){return execFileSync('psql',['-h','127.0.0.1','-p','5433','-U','merchantos','-d','merchantos','-v','ON_ERROR_STOP=1','-Atc',statement],{encoding:'utf8'}).trim();}
function otp(secret:string){return execFileSync('python3',['-c',"import sys;sys.path.insert(0,'reference-pos/scripts');from mfa_fixture import totp;sys.stdout.write(totp(sys.stdin.read().strip()))"],{input:secret,encoding:'utf8'});}
function identity(enroll=true){
 const name='ui-'+randomBytes(8).toString('hex'),password=randomBytes(24).toString('base64url');const dir=mkdtempSync(join(tmpdir(),'merchant-mfa-ui-'));chmodSync(dir,0o700);
 const command=(operation:string,extra:any={})=>execFileSync('dotnet',['run','--project',project,'-c','Release','--no-build','--no-launch-profile','--','--account'],{env:{...process.env,Operations__AllowAccountCommand:'true',Account__Operation:operation,Account__Username:name,Account__Password:password,Account__Role:'Administrator',...extra},stdio:['ignore','pipe','pipe']});
 command('create');let codes:string[]=[];
 if(enroll){
  const secret=execFileSync('python3',['-c','import base64,secrets;print(base64.b32encode(secrets.token_bytes(32)).decode().rstrip("="))'],{encoding:'utf8'}).trim();
  const sf=join(dir,'secret'),cf=join(dir,'confirmation'),out=join(dir,'codes');writeFileSync(sf,secret,{mode:0o600});writeFileSync(cf,otp(secret),{mode:0o600});
  command('mfa-seed',{Operations__AllowMfaEnrollment:'true',Account__MfaSecretFile:sf,Account__CodeFile:cf,Account__RecoveryFile:out});codes=readFileSync(out,'utf8').trim().split('\n');
 }
 return {name,password,codes};
}
async function secretInput(page:Page,id:string,value:string){await page.locator(id).evaluate((el:HTMLInputElement,v:string)=>el.value=v,value);}
async function primary(page:Page,user:ReturnType<typeof identity>){await page.goto('/Account/Login');await page.locator('#Username').fill(user.name);await secretInput(page,'#Password',user.password);await page.getByRole('button',{name:'Sign In',exact:true}).click();await expect(page).toHaveURL(/Account\/Mfa/);}
async function redeem(page:Page,code:string){await secretInput(page,'#RecoveryCode',code);await page.getByRole('button',{name:'Verify',exact:true}).click();}
test('MFA pending identity has no authority; invalid code, CSRF and cancel are safe',async({page})=>{
 const user=identity();await primary(page,user);expect((await page.request.get('/Admin/ProductRows',{maxRedirects:0})).status()).toBe(401);
 const cookies=await page.context().cookies();expect(cookies.some(c=>c.name==='MerchantOS.Admin.Session')).toBeFalsy();expect(cookies.some(c=>c.name==='MerchantOS.Pos.Session')).toBeFalsy();
 expect((await page.request.post('/Account/Verify',{form:{Code:'000000'}})).status()).toBe(400);
 await secretInput(page,'#Code','abcdef');await page.locator('form').evaluate((f:HTMLFormElement)=>f.noValidate=true);await page.getByRole('button',{name:'Verify',exact:true}).click();await expect(page.getByRole('alert')).toContainText('Unable to verify');
 await page.screenshot({path:'test-results/'+test.info().project.name+'/mfa-challenge.png'});expect(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth)).toBeTruthy();
 await page.getByRole('button',{name:'Cancel sign-in'}).click();await expect(page).toHaveURL(/Account\/Login/);expect((await page.request.get('/Admin/ProductRows',{maxRedirects:0})).status()).toBe(401);
});
test('first enrollment confirms real TOTP and displays one-time recovery codes privately',async({page})=>{
 const user=identity(false);await primary(page,user);await expect(page.getByRole('heading')).toHaveText('Set up your authenticator');
 const key=(await page.locator('#ManualKey').textContent())!.trim();const code=otp(key);expect((await page.locator('.mfa-qr').getAttribute('src'))?.startsWith('data:image/svg+xml;base64,')).toBeTruthy();
 await page.screenshot({path:'test-results/'+test.info().project.name+'/mfa-enrollment-redacted.png',mask:[page.locator('[data-sensitive]')]});
 await secretInput(page,'#Code',code);await page.getByRole('button',{name:'Verify',exact:true}).click();await expect(page.getByRole('heading')).toContainText('recovery codes');
 const html=await page.content();expect((html.match(/MRC_[A-F0-9]{64}/g)||[]).length).toBe(10);expect((await page.request.get('/Admin/ProductRows')).status()).toBe(200);
 expect(sql("SELECT mfa_enabled FROM auth_users WHERE username='"+user.name+"'")).toBe('t');
});
test('expired backend challenge cannot create a full MVC session',async({page})=>{
 const user=identity();await primary(page,user);sql("UPDATE auth_challenges SET expires_at=now()-interval '1 second' WHERE user_id=(SELECT id FROM auth_users WHERE username='"+user.name+"')");
 await redeem(page,user.codes[0]);await expect(page.getByRole('alert')).toContainText('Unable to verify');expect((await page.request.get('/Admin/ProductRows',{maxRedirects:0})).status()).toBe(401);
});
test('recovery code works once and a later sign-in rejects its reuse',async({page})=>{
 const user=identity();await primary(page,user);await redeem(page,user.codes[0]);await expect(page).toHaveURL('http://127.0.0.1:5090/');
 await page.context().clearCookies();await primary(page,user);await redeem(page,user.codes[0]);await expect(page.getByRole('alert')).toContainText('Unable to verify');await redeem(page,user.codes[1]);await expect(page).toHaveURL('http://127.0.0.1:5090/');
});
test('password change validates current credential and revokes the authenticated session',async({page})=>{
 const user=identity();await primary(page,user);await redeem(page,user.codes[0]);await page.goto('/Account/Password');const replacement=randomBytes(24).toString('base64url');
 await secretInput(page,'#CurrentPassword','incorrect');await secretInput(page,'#NewPassword',replacement);await secretInput(page,'#ConfirmPassword',replacement);await page.getByRole('button',{name:'Change password',exact:true}).click();await expect(page.getByRole('alert')).toContainText('Password change failed');
 await secretInput(page,'#CurrentPassword',user.password);await secretInput(page,'#NewPassword',replacement);await secretInput(page,'#ConfirmPassword',replacement);await page.getByRole('button',{name:'Change password',exact:true}).click();await expect(page).toHaveURL(/Account\/Login/);expect((await page.request.get('/Admin/ProductRows',{maxRedirects:0})).status()).toBe(401);
});
