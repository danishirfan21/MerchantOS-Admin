// Internal design review: render supplied Stitch HTML and the actual MVC pages at the approved desktop size.
const { chromium } = require('@playwright/test');
const path = require('node:path');
const fs = require('node:fs');
(async()=>{
 const browser=await chromium.launch({headless:true});
 const page=await browser.newPage({viewport:{width:1440,height:900},deviceScaleFactor:1});
 const dir=process.env.VISUAL_REVIEW_DIR || '../desktop-review';fs.mkdirSync(dir,{recursive:true});
 const orders=await (await page.request.get('http://127.0.0.1:5080/api/orders')).json();
 for(const [name,route] of [['overview','/'],['products','/Admin/Products'],['inventory','/Admin/Inventory'],['orders','/Admin/Orders'],['order-details','/Admin/Order/'+orders[0].id]]) {
  await page.goto('http://127.0.0.1:5091/'+name+'.html');await page.evaluate(()=>document.fonts.ready);await page.waitForTimeout(400);
  await page.screenshot({path:path.join(dir,name+'-reference.png')});
  await page.goto('http://127.0.0.1:5090'+route);await page.evaluate(()=>document.fonts.ready);
  await page.screenshot({path:path.join(dir,name+'-mvc.png')});
  console.log(name,JSON.stringify(await page.evaluate(()=>({sidebar:document.querySelector('.sidebar').getBoundingClientRect().width,header:document.querySelector('.topbar').getBoundingClientRect().height,overflow:document.documentElement.scrollWidth>innerWidth,heading:getComputedStyle(document.querySelector('h1')).fontSize,font:getComputedStyle(document.body).fontFamily}))));
 }
 await browser.close();
})();
