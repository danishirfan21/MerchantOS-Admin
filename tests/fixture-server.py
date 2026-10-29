"""Explicit browser-only POS contract fixture; never used by the application or CI live tests.
Run: python3 tests/fixture-server.py ../reference-pos/db/init/002_seed.sql
This does NOT verify PostgreSQL or the actual POS API.
"""
import ast,json,re,sys
from http.server import ThreadingHTTPServer,BaseHTTPRequestHandler
from urllib.parse import urlparse
seed=open(sys.argv[1]).read()
products=[]
for line in seed.splitlines():
 if re.match(r"\('p\d+', '",line):
  row=ast.literal_eval(line.rstrip(',;').replace('NULL','None'))
  products.append(dict(zip(['id','sku','name','nameAr','category','priceHalalas','color','icon','badge'],row)))
stock=dict((key,int(value)) for key,value in re.findall(r"\('(p\d+)', (\d+)\)",seed))
for product in products: product['stock']=stock[product['id']]
order={'id':'41458fda-720a-4070-b66a-ce136ca7d08f','orderNumber':'MOS-FIXTURE','createdAt':'2026-10-30T00:00:00Z','currency':'SAR','status':'completed','subtotalHalalas':429900,'discountHalalas':0,'vatHalalas':64485,'payableHalalas':494385,'paymentMethod':'cash','changeHalalas':5615,'items':[{'productId':'p01','sku':products[0]['sku'],'name':products[0]['name'],'quantity':1,'unitPriceHalalas':429900}]}
class Handler(BaseHTTPRequestHandler):
 def do_GET(self):
  path=urlparse(self.path).path
  value=None
  if path=='/api/products': value=products
  elif path.startswith('/api/products/'): value=next((p for p in products if p['id']==path.split('/')[-1]),None)
  elif path=='/api/orders': value=[order]
  elif path=='/api/orders/'+order['id']: value=order
  self.send_response(200 if value is not None else 404);self.send_header('Content-Type','application/json');self.end_headers();self.wfile.write(json.dumps(value).encode())
 def log_message(self,*args): pass
print('CONTRACT FIXTURE ONLY: not a real POS API or database',flush=True)
ThreadingHTTPServer(('127.0.0.1',5080),Handler).serve_forever()
