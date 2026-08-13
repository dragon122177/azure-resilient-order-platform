export type Status = 'Submitted' | 'InventoryReserved' | 'PaymentAuthorized' | 'ReadyForFulfillment' | 'Shipped' | 'Delivered' | 'Cancelled' | 'Failed'
export type OrderItem = { sku: string; name: string; quantity: number; unitPrice: number; lineTotal: number }
export type Order = { id: string; externalReference: string; customerEmail: string; status: Status; total: number; currency: string; items: OrderItem[]; countryCode: string; paymentReference?: string; carrier?: string; trackingNumber?: string; failureReason?: string; createdAt: string; updatedAt: string; completedAt?: string }
export type PagedOrders = { items: Order[]; page: number; pageSize: number; totalCount: number }
export type Metrics = { totalOrders: number; activeOrders: number; completedOrders: number; failedOrders: number; grossValue: number; byStatus: Record<string, number> }
export type InventoryItem = { sku: string; name: string; availableQuantity: number; reservedQuantity: number; updatedAt: string }
export type AuditEntry = { id: string; actor: string; action: string; resourceType: string; resourceId: string; correlationId: string; occurredAt: string; details?: string }
