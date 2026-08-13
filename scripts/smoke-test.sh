#!/usr/bin/env bash
set -euo pipefail

base_url="${ORDERGRID_BASE_URL:-http://localhost:8080}"
auth_mode="${ORDERGRID_SMOKE_AUTH_MODE:-demo}"
key="smoke-$(date +%s)-00000000"

curl --fail --silent --show-error "${base_url}/health/ready" >/dev/null
if [[ "${auth_mode}" == "health-only" ]]; then
  echo "Smoke test passed: readiness dependency is healthy."
  exit 0
fi

auth_headers=(--header 'X-Tenant-ID: demo')
if [[ "${auth_mode}" == "bearer" ]]; then
  [[ -n "${ORDERGRID_ACCESS_TOKEN:-}" ]] || { echo "ORDERGRID_ACCESS_TOKEN is required" >&2; exit 1; }
  auth_headers=(--header "Authorization: Bearer ${ORDERGRID_ACCESS_TOKEN}")
fi

response="$(curl --fail --silent --show-error --request POST \
  --header 'Content-Type: application/json' \
  --header "Idempotency-Key: ${key}" \
  "${auth_headers[@]}" --data @samples/create-order.json "${base_url}/api/v1/orders")"
order_id="$(printf '%s' "${response}" | sed -n 's/.*"id":"\([^"]*\)".*/\1/p')"
[[ -n "${order_id}" ]] || { echo "Create response contained no order id" >&2; exit 1; }
curl --fail --silent --show-error "${auth_headers[@]}" "${base_url}/api/v1/orders/${order_id}" >/dev/null
echo "Smoke test passed for order ${order_id}."
