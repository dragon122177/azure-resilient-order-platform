import { useEffect, useMemo, useState } from 'react'
import { loadDashboard } from './api'
import type { AuditEntry, InventoryItem, Metrics, Order, PagedOrders } from './types'
import { filterOrders, stockPressure } from './utils'

type Data = { metrics: Metrics; orders: PagedOrders; inventory: InventoryItem[]; audit: AuditEntry[]; source: 'loading' | 'api' | 'demo' }
const initial: Data = { metrics: { totalOrders: 0, activeOrders: 0, completedOrders: 0, failedOrders: 0, grossValue: 0, byStatus: {} }, orders: { items: [], page: 1, pageSize: 20, totalCount: 0 }, inventory: [], audit: [], source: 'loading' }
const money = new Intl.NumberFormat('ja-JP', { style: 'currency', currency: 'JPY', maximumFractionDigits: 0 })
const compact = new Intl.NumberFormat('en', { notation: 'compact', maximumFractionDigits: 1 })
const relative = new Intl.RelativeTimeFormat('en', { numeric: 'auto' })
function ago(value: string) { const minutes = Math.round((new Date(value).getTime() - Date.now()) / 60_000); return Math.abs(minutes) < 60 ? relative.format(minutes, 'minute') : relative.format(Math.round(minutes / 60), 'hour') }
function Status({ value }: { value: Order['status'] }) { return <span className={`status status-${value.toLowerCase()}`}><i />{value.replace(/([a-z])([A-Z])/g, '$1 $2')}</span> }
function Metric({ label, value, note, tone }: { label: string; value: string; note: string; tone: string }) { return <article className={`metric ${tone}`}><div><span>{label}</span><b>↗</b></div><strong>{value}</strong><p>{note}</p><i className="spark" /></article> }

export function App() {
  const [data, setData] = useState<Data>(initial)
  const [loading, setLoading] = useState(true)
  const [query, setQuery] = useState('')
  const [selected, setSelected] = useState<Order | null>(null)
  const refresh = () => { setLoading(true); loadDashboard().then(value => { setData(value); setLoading(false) }) }
  useEffect(refresh, [])
  const visible = useMemo(() => filterOrders(data.orders.items, query), [data.orders.items, query])
  return <div className="shell">
    <aside className="sidebar"><div className="brand"><span className="logo">OG</span><div><strong>OrderGrid</strong><small>Azure Operations</small></div></div>
      <nav>{['Overview', 'Orders', 'Inventory', 'Event stream', 'Audit log'].map((x, i) => <button className={i === 0 ? 'active' : ''} key={x}><span>{['⌘','□','◇','↻','✓'][i]}</span>{x}</button>)}</nav>
      <div className="environment"><i /><div><small>Environment</small><strong>Reference · Japan East</strong></div></div>
    </aside>
    <main><header><div className="search">⌕ <input aria-label="Search" placeholder="Search orders, customers, events…" value={query} onChange={e => setQuery(e.target.value)} /></div><div className="profile"><span>VV</span><div><b>Victor Villamar</b><small>Platform Operator</small></div></div></header>
      <div className="content"><section className="heading"><div><p>OPERATIONS CONTROL PLANE</p><h1>Good morning, Victor.</h1><span>{data.source === 'api' ? 'Showing a current API snapshot.' : data.source === 'demo' ? 'Showing clearly labeled synthetic fallback data.' : 'Loading the operations snapshot…'}</span></div><div className="actions"><em><i />{data.source === 'api' ? 'API data' : data.source === 'demo' ? 'Demo data' : 'Loading'}</em><button onClick={refresh}>↻ Refresh</button><button className="primary" onClick={() => window.open('/openapi/v1.json')}>API contract ↗</button></div></section>
        <section className="metrics"><Metric label="Orders processed" value={compact.format(data.metrics.totalOrders)} note="Current tenant total" tone="blue" /><Metric label="Active workflows" value={`${data.metrics.activeOrders}`} note="Across workflow stages" tone="purple" /><Metric label="Gross order value" value={money.format(data.metrics.grossValue)} note="Synthetic in fallback mode" tone="cyan" /><Metric label="Requires attention" value={`${data.metrics.failedOrders}`} note="Failed workflows" tone="orange" /></section>
        <section className="grid"><article className="panel orders"><div className="panel-title"><div><h2>Order flow</h2><p>Asynchronous orchestration across every stage</p></div><button>View all →</button></div>
          <div className="flow">{['Submitted','Inventory','Payment','Fulfillment','Shipped'].map((stage, i) => <div key={stage}><span>{Object.values(data.metrics.byStatus)[i] ?? [31,42,18,38,18][i]}</span><b>{stage}</b><small>{['Awaiting reservation','Stock reserved','Authorized','Ready to dispatch','In transit'][i]}</small></div>)}</div>
          <div className="tabs"><b>Recent orders</b><span>Needs attention <i>{data.metrics.failedOrders}</i></span></div>
          <div className="table"><table><thead><tr><th>Order</th><th>Customer</th><th>Status</th><th>Value</th><th>Updated</th></tr></thead><tbody>{loading ? <tr><td colSpan={5}>Loading operations data…</td></tr> : visible.map(order => <tr key={order.id} onClick={() => setSelected(order)}><td><b>{order.externalReference}</b><small>{order.items.length} item · {order.countryCode}</small></td><td>{order.customerEmail}</td><td><Status value={order.status} /></td><td><b>{money.format(order.total)}</b></td><td>{ago(order.updatedAt)}</td></tr>)}</tbody></table></div>
        </article><aside className="right"><article className="panel"><div className="panel-title"><div><h2>Service map</h2><p>Reference Azure topology</p></div><em>IaC defined</em></div>{[['Container Apps','API & Worker'],['Service Bus','order-events'],['Azure SQL','Primary database'],['Blob Storage','Projections']].map(([a,b]) => <div className="service" key={a}><span>{a[0]}</span><div><b>{a}</b><small>{b}</small></div><em>● Configured</em></div>)}</article>
          <article className="panel activity"><div className="panel-title"><div><h2>Recent activity</h2><p>Audit-backed workflow events</p></div></div>{data.audit.map(entry => <div className="event" key={entry.id}><i /><div><b>{entry.action.replaceAll('.', ' · ')}</b><p>{entry.resourceId} by {entry.actor}</p><small>{ago(entry.occurredAt)} · {entry.correlationId}</small></div></div>)}</article></aside></section>
        <section className="panel inventory"><div className="panel-title"><div><h2>Inventory snapshot</h2><p>Reservation pressure and available stock</p></div></div><div className="inventory-grid">{data.inventory.map(item => <div key={item.sku}><span>{item.sku}</span><small>{item.name}</small><strong>{item.availableQuantity}<i> available</i></strong><div><i style={{ width: `${Math.max(stockPressure(item), 3)}%` }} /></div><p>{item.reservedQuantity} reserved · {ago(item.updatedAt)}</p></div>)}</div></section>
      </div>
    </main>
    {selected && <div className="backdrop" onClick={() => setSelected(null)}><aside className="drawer" onClick={e => e.stopPropagation()}><button onClick={() => setSelected(null)}>×</button><p>ORDER DETAIL</p><h2>{selected.externalReference}</h2><Status value={selected.status} /><dl><dt>Customer</dt><dd>{selected.customerEmail}</dd><dt>Value</dt><dd>{money.format(selected.total)}</dd><dt>Country</dt><dd>{selected.countryCode}</dd></dl><h3>Line items</h3>{selected.items.map(item => <div className="line" key={item.sku}><span><b>{item.name}</b><small>{item.sku} · qty {item.quantity}</small></span><b>{money.format(item.lineTotal)}</b></div>)}</aside></div>}
  </div>
}
