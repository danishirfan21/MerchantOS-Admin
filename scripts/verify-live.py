import sys,os,http.cookiejar,re
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]/"reference-pos/scripts"))
from auth_client import Client
pos_auth=Client("ADMIN")
"""Read-only integration verification against the pinned POS fixture and Admin."""
import json, urllib.request
POS='http://127.0.0.1:5080'; ADMIN='http://127.0.0.1:5090'
def read(url):
 with pos_auth.opener.open(url,timeout=15) as response: return response.read().decode()
def data(path): return json.loads(read(POS+path))
login_html=read(ADMIN+'/Account/Login')
csrf=re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',login_html).group(1)
from urllib.parse import urlencode
req=urllib.request.Request(ADMIN+'/Account/Login',data=urlencode({'Username':os.environ['TEST_ADMIN_USERNAME'],'Password':os.environ['TEST_ADMIN_PASSWORD'],'ReturnUrl':'/','__RequestVerificationToken':csrf}).encode())
with pos_auth.opener.open(req,timeout=20) as response:
 assert response.status==200;factor_html=response.read().decode()
csrf=re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',factor_html).group(1)
from mfa_fixture import take
req=urllib.request.Request(ADMIN+'/Account/Verify',data=urlencode({'RecoveryCode':take(os.environ['TEST_ADMIN_RECOVERY_FILE']),'__RequestVerificationToken':csrf}).encode())
with pos_auth.opener.open(req,timeout=20) as response:assert response.status==200
products=data('/api/products'); assert len(products)==30
for product in products:
 assert data('/api/inventory/'+product['id'])['stock']==product['stock']
 assert data('/api/products/'+product['id'])['sku']==product['sku']
rows=[]
for page in range(1,6):
 result=json.loads(read(ADMIN+'/Admin/ProductRows?page='+str(page)))
 rows.extend(result['products'])
assert len(rows)==len(products)
assert len({row['id'] for row in rows})==len(products)
assert sum(row['stock'] for row in rows)==sum(product['stock'] for product in products)
orders=data('/api/orders'); assert orders, 'Fixture must contain a real completed checkout'
for order in orders:
 receipt=data('/api/orders/'+order['id']); assert receipt['items']
 html=read(ADMIN+'/Admin/Order/'+order['id']); assert order['orderNumber'] in html
for path in ['/', '/Admin/Products', '/Admin/Inventory', '/Admin/Orders']:
 assert 'Unable to load POS data.' not in read(ADMIN+path)
print(f'Live integration passed: {len(products)} catalog/detail/inventory records, {len(orders)} order receipts, four Razor pages.')
