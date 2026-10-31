"""Add receipts only to the disposable GitHub Actions POS database for pagination coverage."""
import json
import os
import urllib.request
import sys
sys.path.insert(0,str(__import__("pathlib").Path(__file__).resolve().parents[1]/"reference-pos/scripts"))
from auth_client import Client
auth=Client("CASHIER")

if os.environ.get("GITHUB_ACTIONS") != "true":
    raise SystemExit("This fixture may run only in the disposable GitHub Actions integration job.")

base = "http://127.0.0.1:5080"
for index in range(5):
    payload = {
        "items": [{"productId": "p17", "quantity": 1}],
        "discountPercent": 0,
        "paymentMethod": "cash",
        "cashTenderedHalalas": 1000000,
        "idempotencyKey": "admin-browser-pagination-" + str(index),
    }
    status, receipt = auth.request("/api/checkout",payload)
    assert status == 200 and receipt["status"] == "completed"
assert len(auth.request("/api/receipts")[1]) >= 10
print("Disposable CI browser fixture ready: at least ten persisted receipts for real order pagination.")
