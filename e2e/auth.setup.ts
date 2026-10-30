import {request} from '@playwright/test';
import {writeFileSync} from 'node:fs';
export default async function setup(){
 const api=await request.newContext();const base='http://127.0.0.1:5080';
 const token=(await (await api.get(base+'/api/auth/csrf')).json()).token;
 const login=await api.post(base+'/api/auth/login',{headers:{'X-CSRF-TOKEN':token},data:{username:process.env['TEST_ADMIN_USERNAME'],password:process.env['TEST_ADMIN_PASSWORD']}});
 if(!login.ok())throw new Error('API browser fixture sign-in failed');
 const page=await api.get('http://127.0.0.1:5090/Account/Login');const html=await page.text();const csrf=/name="__RequestVerificationToken"[^>]*value="([^"]+)"/.exec(html)?.[1];
 if(!csrf)throw new Error('Missing MVC antiforgery token');
 const result=await api.post('http://127.0.0.1:5090/Account/Login',{form:{Username:process.env['TEST_ADMIN_USERNAME']!,Password:process.env['TEST_ADMIN_PASSWORD']!,ReturnUrl:'/',__RequestVerificationToken:csrf}});
 if(!result.ok())throw new Error('Admin browser fixture sign-in failed');
 writeFileSync('/tmp/merchant-admin-state.json',JSON.stringify(await api.storageState()),{mode:0o600});await api.dispose();
}
