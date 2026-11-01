import {execFileSync} from 'node:child_process';
import {resolve} from 'node:path';
export function recovery(){const path=process.env['TEST_ADMIN_RECOVERY_FILE'];if(!path)throw new Error('Missing private recovery fixture');return execFileSync('python3',[resolve('reference-pos/scripts/mfa_fixture.py'),'--take',path],{encoding:'utf8'}).trim();}
export async function completeMfa(page:any){await page.waitForURL(/Account\/Mfa/);const code=recovery();await page.locator('#RecoveryCode').evaluate((el:HTMLInputElement,value:string)=>el.value=value,code);await page.getByRole('button',{name:'Verify',exact:true}).click();}
export async function mvcLogin(api:any,returnUrl='/'){
 const page=await api.get('http://127.0.0.1:5090/Account/Login');const csrf=/name="__RequestVerificationToken"[^>]*value="([^"]+)"/.exec(await page.text())?.[1];if(!csrf)throw new Error('Missing MVC CSRF');
 const result=await api.post('http://127.0.0.1:5090/Account/Login',{form:{Username:process.env['TEST_ADMIN_USERNAME']!,Password:process.env['TEST_ADMIN_PASSWORD']!,ReturnUrl:returnUrl,__RequestVerificationToken:csrf}});
 const text=await result.text();const token=/name="__RequestVerificationToken"[^>]*value="([^"]+)"/.exec(text)?.[1];if(!token)throw new Error('Missing MFA CSRF');
 const final=await api.post('http://127.0.0.1:5090/Account/Verify',{form:{RecoveryCode:recovery(),__RequestVerificationToken:token}});if(!final.ok())throw new Error('Real MFA fixture sign-in failed');return final;
}
