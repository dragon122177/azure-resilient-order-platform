import type { Order } from './types'
export function filterOrders(orders: Order[], query: string) {
  const value = query.trim().toLowerCase()
  return value ? orders.filter(order => [order.externalReference, order.customerEmail, order.status]
    .some(field => field.toLowerCase().includes(value))) : orders
}
export function stockPressure(item: { availableQuantity: number; reservedQuantity: number }) {
  const total = item.availableQuantity + item.reservedQuantity
  return total === 0 ? 0 : Math.round((item.reservedQuantity / total) * 100)
}
