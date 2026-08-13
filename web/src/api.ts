import type { AuditEntry, InventoryItem, Metrics, PagedOrders } from './types'

const headers = { 'X-Tenant-ID': 'demo', 'X-Demo-User': 'operations.console' }
async function get<T>(path: string, fallback: T): Promise<{ value: T; live: boolean }> {
  try {
    const response = await fetch(path, { headers })
    if (!response.ok) throw new Error(`${response.status}`)
    return { value: await response.json() as T, live: true }
  } catch { return { value: fallback, live: false } }
}

export async function loadDashboard() {
  const [metrics, orders, inventory, audit] = await Promise.all([
    get<Metrics>('/api/v1/operations/metrics', demoMetrics),
    get<PagedOrders>('/api/v1/orders?page=1&pageSize=20', demoOrders),
    get<InventoryItem[]>('/api/v1/operations/inventory', demoInventory),
    get<AuditEntry[]>('/api/v1/operations/audit?limit=8', demoAudit),
  ])
  return { metrics: metrics.value, orders: orders.value, inventory: inventory.value,
    audit: audit.value, source: [metrics, orders, inventory, audit].every(x => x.live) ? 'api' : 'demo' } as const
}

const ago = (minutes: number) => new Date(Date.now() - minutes * 60_000).toISOString()
const items = (sku: string, name: string, quantity: number, price: number) =>
  [{ sku, name, quantity, unitPrice: price, lineTotal: price * quantity }]
export const demoMetrics: Metrics = { totalOrders: 2481, activeOrders: 147, completedOrders: 2310,
  failedOrders: 24, grossValue: 28645800, byStatus: { Submitted: 31, InventoryReserved: 42,
    PaymentAuthorized: 18, ReadyForFulfillment: 38, Shipped: 18, Delivered: 2310, Failed: 24 } }
export const demoOrders: PagedOrders = { page: 1, pageSize: 20, totalCount: 2481, items: [
  { id: 'ef82c719-d4ec-4a67-8b5b-bfb2ee95a2a0', externalReference: 'WEB-2026-2481', customerEmail: 'aiko.tanaka@example.jp', status: 'ReadyForFulfillment', total: 12400, currency: 'JPY', countryCode: 'JP', paymentReference: 'sim_ef82', createdAt: ago(4), updatedAt: ago(2), items: items('AZ-100', 'Azure Architecture Workbook', 2, 6200) },
  { id: 'af82c719-d4ec-4a67-8b5b-bfb2ee95a2a1', externalReference: 'API-2026-2480', customerEmail: 'kenji.sato@example.jp', status: 'InventoryReserved', total: 8400, currency: 'JPY', countryCode: 'JP', createdAt: ago(8), updatedAt: ago(7), items: items('SB-200', 'Service Bus Reliability Kit', 1, 8400) },
  { id: 'bf82c719-d4ec-4a67-8b5b-bfb2ee95a2a2', externalReference: 'POS-2026-2479', customerEmail: 'mika.ito@example.jp', status: 'Shipped', total: 18500, currency: 'JPY', countryCode: 'JP', carrier: 'Yamato', trackingNumber: 'YG-80210642', createdAt: ago(35), updatedAt: ago(12), items: items('DOTNET-10', '.NET Cloud Engineering Guide', 2, 9250) },
  { id: 'cf82c719-d4ec-4a67-8b5b-bfb2ee95a2a3', externalReference: 'WEB-2026-2478', customerEmail: 'decline.demo@example.jp', status: 'Failed', total: 6800, currency: 'JPY', countryCode: 'JP', failureReason: 'Payment simulator declined the order.', createdAt: ago(48), updatedAt: ago(46), completedAt: ago(46), items: items('OBS-300', 'Observability Field Manual', 1, 6800) },
] }
export const demoInventory: InventoryItem[] = [
  { sku: 'AZ-100', name: 'Azure Architecture Workbook', availableQuantity: 250, reservedQuantity: 18, updatedAt: ago(2) },
  { sku: 'DOTNET-10', name: '.NET Cloud Engineering Guide', availableQuantity: 180, reservedQuantity: 12, updatedAt: ago(5) },
  { sku: 'SB-200', name: 'Service Bus Reliability Kit', availableQuantity: 120, reservedQuantity: 9, updatedAt: ago(7) },
  { sku: 'OBS-300', name: 'Observability Field Manual', availableQuantity: 90, reservedQuantity: 4, updatedAt: ago(12) },
]
export const demoAudit: AuditEntry[] = [
  { id: '1', actor: 'order-orchestrator', action: 'workflow.fulfillment_ready', resourceType: 'order', resourceId: '…a2a0', correlationId: 'corr-e319', occurredAt: ago(2) },
  { id: '2', actor: 'order-orchestrator', action: 'workflow.payment_authorized', resourceType: 'order', resourceId: '…a2a0', correlationId: 'corr-e319', occurredAt: ago(3) },
  { id: '3', actor: 'operations.console', action: 'order.shipped', resourceType: 'order', resourceId: '…a2a2', correlationId: 'corr-f711', occurredAt: ago(12) },
]
