"""Read-only integration verification against the pinned POS fixture and Admin."""
import json, urllib.request
POS='http://127.0.0.1:5080'; ADMIN='http://127.0.0.1:5090'
def read(url):
 with urllib.request.urlopen(url,timeout=15) as response: return response.read().decode()
def data(path): return json.loads(read(POS+path))
products=data('/api/products'); assert len(products)==30
for product in products:
 assert data('/api/inventory/'+product['id'])['stock']==product['stock']
 assert data('/api/products/'+product['id'])['sku']==product['sku']
rows=json.loads(read(ADMIN+'/Admin/ProductRows')); assert len(rows)==len(products)
assert sum(row['stock'] for row in rows)==sum(product['stock'] for product in products)
orders=data('/api/orders'); assert orders, 'Fixture must contain a real completed checkout'
for order in orders:
 receipt=data('/api/orders/'+order['id']); assert receipt['items']
 html=read(ADMIN+'/Admin/Order/'+order['id']); assert order['orderNumber'] in html
for path in ['/', '/Admin/Products', '/Admin/Inventory', '/Admin/Orders']:
 assert 'Unable to load POS data.' not in read(ADMIN+path)
print(f'Live integration passed: {len(products)} catalog/detail/inventory records, {len(orders)} order receipts, four Razor pages.')
