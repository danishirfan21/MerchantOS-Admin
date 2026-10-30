const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const Handlebars = require('handlebars');
const render = Handlebars.compile(fs.readFileSync('assets/templates/products.hbs', 'utf8'));
test('dynamic product values are HTML escaped', () => {
 const html = render({products:[{name:'<img src=x onerror=alert(1)>',sku:'" onclick="alert(1)',detailUrl:'/Admin/Product/a',stock:0}]});
 assert.ok(html.includes('&lt;img')); assert.ok(!html.includes('<img src=x onerror=alert(1)>')); assert.ok(!html.includes('sku">" onclick'));
});
test('empty filtered catalog renders an honest empty state', () => {
 assert.ok(render({products:[]}).includes('No products found'));
});
test('compiled template works with CSP-compatible runtime', () => {
 require('node:vm').runInNewContext(fs.readFileSync('src/MerchantOS.Admin/wwwroot/js/templates.js','utf8'),{Handlebars});
 assert.ok(Handlebars.templates.products({products:[]}).includes('No products found'));
});
