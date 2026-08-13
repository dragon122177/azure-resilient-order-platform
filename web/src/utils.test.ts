import { describe, expect, it } from 'vitest'
import { filterOrders, stockPressure } from './utils'
import type { Order } from './types'
const order = { externalReference: 'WEB-100', customerEmail: 'aiko@example.jp', status: 'Submitted' } as Order
describe('dashboard utilities', () => {
  it('filters by reference', () => expect(filterOrders([order], 'web-100')).toHaveLength(1))
  it('filters by customer', () => expect(filterOrders([order], 'nobody')).toHaveLength(0))
  it('calculates reservation pressure', () => expect(stockPressure({ availableQuantity: 75, reservedQuantity: 25 })).toBe(25))
})
