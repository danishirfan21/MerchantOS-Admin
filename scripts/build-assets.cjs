const fs = require('node:fs');
const path = require('node:path');
const Handlebars = require('handlebars');
const destination = 'src/MerchantOS.Admin/wwwroot/vendor';
fs.mkdirSync(destination, {recursive: true});
for (const [source, name] of [
 ['bootstrap/dist/css/bootstrap.min.css', 'bootstrap.min.css'],
 ['bootstrap/dist/js/bootstrap.bundle.min.js', 'bootstrap.bundle.min.js'],
 ['jquery/dist/jquery.min.js', 'jquery.min.js'],
 ['handlebars/dist/handlebars.runtime.min.js', 'handlebars.runtime.min.js']
]) fs.copyFileSync(path.join('node_modules', source), path.join(destination, name));
const compiled = Handlebars.precompile(fs.readFileSync('assets/templates/products.hbs', 'utf8'));
fs.writeFileSync('src/MerchantOS.Admin/wwwroot/js/templates.js',
 "Handlebars.templates = { products: Handlebars.template(" + compiled + ") };\n");
console.log('Copied Bootstrap, jQuery, Handlebars runtime and compiled escaped product template.');
