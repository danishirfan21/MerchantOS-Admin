import { defineConfig, devices } from '@playwright/test';
export default defineConfig({ testDir: './e2e', outputDir: './test-results/browser', retries: 0, workers: 1,
 reporter: [['list'], ['html', {open: 'never'}]],
 use: {baseURL: 'http://127.0.0.1:5090', screenshot: 'on', trace: 'retain-on-failure'},
 projects: [ {name:'desktop',use:{...devices['Desktop Chrome'],viewport:{width:1440,height:900}}},
 {name:'tablet',use:{...devices['iPad Mini'],defaultBrowserType:'chromium'}},
 {name:'mobile',use:{...devices['iPhone 13'],defaultBrowserType:'chromium'}} ] });
