"""Add receipts only to the disposable GitHub Actions POS database for pagination coverage."""
import json
import os
import urllib.request

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
    request = urllib.request.Request(base + "/api/checkout", data=json.dumps(payload).encode(),
                                     headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(request, timeout=20) as response:
        assert response.status == 200
        assert json.load(response)["status"] == "completed"
with urllib.request.urlopen(base + "/api/orders", timeout=20) as response:
    assert len(json.load(response)) >= 10
print("Disposable CI browser fixture ready: at least ten persisted receipts for real order pagination.")
