# Desktop design assets

- Design authority: the five Google Stitch HTML files attached by the user on 30 October 2026.
- Five local illustrative product photos were copied from the provided Product Catalog source: the iPhone 15 Pro, Galaxy S24 Ultra, AirPods Pro, Sony WH-1000XM5 and Galaxy Watch6 assets. They are decoration, not API-provided product photography or a claim of exact product likeness. They do not introduce alternate catalog records.
- `Display.Thumbnail` maps matching seeded product IDs to those local files. Other products use the existing POS category SVG illustrations from commit `68cc809edb2146e13632ff9cd316504b2dc5d25d`.
- Inter static font weights 400/500/600/700 were downloaded from Google Fonts and subset to Latin/UI punctuation. OFL license: `src/MerchantOS.Admin/wwwroot/fonts/Inter-LICENSE.txt`.
- Material Symbols Outlined was downloaded from Google Fonts as a small glyph subset. Apache license: `src/MerchantOS.Admin/wwwroot/fonts/Material-Symbols-LICENSE.txt`.
- All application fonts, icons, thumbnails, CSS and JavaScript are served locally under the existing restrictive CSP. No Google Fonts or Tailwind CDN request is required at runtime.

The optional `scripts/visual-review.cjs` expects the supplied reference HTML (named `overview.html`, `products.html`, `inventory.html`, `orders.html`, `order-details.html`) and assets to be served on local port 5091. It captures reference and actual MVC pages at 1440 × 900 into `../desktop-review`. This is a manual comparison helper; CI uses the actual app's Playwright suite without a mock/reference application.
