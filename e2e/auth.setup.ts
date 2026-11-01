import {request} from '@playwright/test';
import {writeFileSync} from 'node:fs';
import {recovery,mvcLogin} from './mfa.fixture';
export default async function setup(){
 const api=await request.newContext();const base='http://127.0.0.1:5080';const token=(await(await api.get(base+'/api/auth/csrf')).json()).token;
 const login=await api.post(base+'/api/auth/login',{headers:{'X-CSRF-TOKEN':token},data:{username:process.env['TEST_ADMIN_USERNAME'],password:process.env['TEST_ADMIN_PASSWORD']}});if(login.status()!==202)throw new Error('Administrator primary fixture must remain restricted');
 const factor=await api.post(base+'/api/auth/mfa/complete',{headers:{'X-CSRF-TOKEN':token},data:{recoveryCode:recovery()}});if(!factor.ok())throw new Error('API MFA fixture failed');
 await mvcLogin(api);writeFileSync('/tmp/merchant-admin-state.json',JSON.stringify(await api.storageState()),{mode:0o600});await api.dispose();
}
