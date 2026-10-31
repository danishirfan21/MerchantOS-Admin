import { defineConfig, devices } from '@playwright/test';
export default defineConfig({ globalSetup:'./e2e/auth.setup.ts', testDir: './e2e', outputDir: './test-results/browser', retries: 0, workers: 1,
 reporter: [['list'], ['html', {open: 'never'}]],
 use: {baseURL: 'http://127.0.0.1:5090', screenshot: 'on', trace: 'off', storageState:'/tmp/merchant-admin-state.json'},
 projects: [ {name:'desktop',testIgnore:['**/tablet.spec.ts','**/mobile.spec.ts'],use:{...devices['Desktop Chrome'],viewport:{width:1440,height:900}}},
 {name:'tablet',testIgnore:'**/mobile.spec.ts',use:{...devices['iPad Mini'],viewport:{width:768,height:1024},deviceScaleFactor:1,defaultBrowserType:'chromium'}},
 {name:'mobile',testMatch:['**/mobile.spec.ts','**/auth.spec.ts','**/management.spec.ts','**/reporting.spec.ts'],use:{...devices['iPhone 13'],viewport:{width:390,height:844},deviceScaleFactor:1,defaultBrowserType:'chromium'}} ] });
