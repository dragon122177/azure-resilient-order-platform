# API guide

The local profile uses demo authentication. Send `X-Tenant-ID` to choose the
synthetic tenant and `X-Demo-Roles` for roles. Azure mode validates Entra tokens;
clients must never be allowed to assert tenant or roles through headers there.

## Create an order

```http
POST /api/v1/orders HTTP/1.1
Content-Type: application/json
Idempotency-Key: checkout-20260813-0001
X-Tenant-ID: demo

{
  "externalReference": "WEB-10042",
  "customerEmail": "operator@example.test",
  "shippingAddress": {
    "recipient": "Demo Operator",
    "line1": "1 Cloud Way",
    "line2": null,
    "city": "Tokyo",
    "region": "Tokyo",
    "postalCode": "100-0001",
    "countryCode": "JP"
  },
  "items": [
    { "sku": "ORDERGRID-MUG", "name": "OrderGrid Mug", "quantity": 2, "unitPrice": 24.50, "currency": "USD" }
  ]
}
```

The key is required and scoped to a tenant. Repeating an identical request returns
the stored body and sets `Idempotency-Replayed: true`. Reusing the key for a
different request returns `409 Conflict`.

## Endpoints

| Method | Route | Policy |
|---|---|---|
| GET | `/api/v1/orders` | `orders.read` |
| GET | `/api/v1/orders/{id}` | `orders.read` |
| POST | `/api/v1/orders` | `orders.write` |
| POST | `/api/v1/orders/{id}/cancel` | `orders.write` |
| POST | `/api/v1/orders/{id}/ship` | `orders.write` |
| POST | `/api/v1/orders/{id}/deliver` | `orders.write` |
| GET | `/api/v1/operations/metrics` | `operations.read` |
| GET | `/api/v1/operations/inventory` | `operations.read` |
| GET | `/api/v1/operations/audit` | `operations.read` |

Pagination is one-based and bounded. Errors use Problem Details and include the
correlation ID. Supply `X-Correlation-ID` for cross-system tracing or let the API
generate one. The generated contract is at `/openapi/v1.json`.

## State-changing examples

```bash
curl -X POST http://localhost:8080/api/v1/orders/$ORDER_ID/ship \
  -H 'Content-Type: application/json' -H 'X-Tenant-ID: demo' \
  -d '{"carrier":"Demo Express","trackingNumber":"DX-12345"}'

curl -X POST http://localhost:8080/api/v1/orders/$ORDER_ID/deliver \
  -H 'X-Tenant-ID: demo'
```

Illegal state transitions return `409`; missing tenant resources return `404` so
the API does not reveal another tenant's identifiers.
