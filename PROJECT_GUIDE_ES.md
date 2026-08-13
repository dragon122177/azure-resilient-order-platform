# Guía de estudio — OrderGrid

Esta guía explica el proyecto en palabras sencillas para poder presentarlo con
honestidad. OrderGrid es una implementación de referencia: muestra cómo diseñar
una plataforma de pedidos resistente en Azure, pero usa datos sintéticos y un
simulador de pago; no afirma haber atendido tráfico real.

## Resumen de 30 segundos

"Construí una plataforma de pedidos en .NET con arquitectura por capas. La API
guarda el pedido y un mensaje de salida en una misma transacción. Un worker
publica ese mensaje en Service Bus y procesa el flujo de inventario, pago
simulado y preparación. La solución evita duplicados con idempotencia y
outbox/inbox, separa tenants, usa identidades administradas, despliega con Bicep
y deja rastros correlacionados en Azure Monitor. También incluye pruebas y una
consola React para operaciones."

## Flujo principal

1. El cliente manda `POST /api/v1/orders` con `Idempotency-Key` y tenant.
2. La API valida el comando y busca una respuesta previa para esa misma llave.
3. EF Core guarda pedido, auditoría y evento de outbox en una sola transacción.
4. El dispatcher toma eventos pendientes y los publica en un tópico de Service Bus.
5. La sesión usa el ID del pedido para conservar el orden de sus eventos.
6. El consumidor reserva inventario, autoriza un pago simulado y prepara fulfillment.
7. Inbox evita procesar dos veces el mismo mensaje; Blob conserva proyecciones.
8. Correlation ID une petición HTTP, auditoría, evento, mensaje y telemetría.

## Patrones que debes poder explicar

### Idempotencia

La llave representa una intención del cliente. Se guarda junto con el hash del
body y la respuesta. Repetir llave y body devuelve la misma respuesta; reutilizar
la llave con otro body produce `409`. Esto evita pedidos dobles por reintentos.

### Transactional outbox

Guardar primero en SQL y publicar después puede perder mensajes. Publicar primero
y guardar después puede emitir eventos de algo que no existió. El outbox guarda
estado y "intención de publicar" en la misma transacción; un worker publica luego.
La entrega sigue siendo al menos una vez, por eso el consumidor también usa inbox.

### Compensación

No hay una transacción distribuida entre inventario y pago. Si el pago simulado
falla, el workflow libera la reserva y marca el pedido como fallido dentro de la
transacción local. En un sistema real cada integración tendría compensaciones y
políticas de reintento explícitas.

### Multi-tenant

El tenant proviene del token (o header únicamente en modo demo) y se aplica en
repositorios/consultas. Las claves únicas incluyen `TenantId`. Antes de producción
agregaría Row-Level Security y pruebas de aislamiento ofensivas.

## ¿Por qué estos servicios Azure?

| Servicio | Responsabilidad |
|---|---|
| Container Apps | Ejecutar API y workers con revisiones y escalado administrado |
| Azure SQL | Transacciones, invariantes relacionales, outbox e inbox |
| Service Bus | Pub/sub durable, sesiones, reintentos y dead-letter queue |
| Blob Storage | Recibos/proyecciones sin disco compartido |
| Key Vault | Guardar secretos de bootstrap sin ponerlos en código |
| Managed Identity | Autenticar cargas sin credenciales permanentes |
| Application Insights | Trazas, métricas y logs correlacionados |
| Log Analytics / Alerts | Investigación operativa y alertas accionables |
| ACR | Imágenes privadas e inmutables por SHA |

## Preguntas probables

**¿Por qué no microservicios?** El dominio aún cabe en una base transaccional y
un despliegue modular. Separé límites para poder extraer componentes cuando el
escalado, la propiedad del equipo o la disponibilidad lo justifiquen.

**¿Es exactly-once?** No. Service Bus y el outbox entregan al menos una vez. El
efecto observable se aproxima a exactamente una vez usando idempotencia e inbox.

**¿Qué harías antes de producción?** Red privada, APIM/Front Door/WAF, SQL con
Entra solamente, migraciones separadas del arranque, pruebas de carga y caos,
backups restaurados, SLOs medidos y revisión de seguridad.

**¿Por qué hay Functions y worker?** El worker es la ruta principal. Functions
es una extensión opcional que demuestra triggers y temporizadores sin duplicar
la responsabilidad del consumidor en el despliegue predeterminado.

**¿Qué parte es simulada?** El proveedor de pagos, datos demo y métricas fallback
de la consola. Están etiquetados y no se presentan como integración productiva.

## Demostración rápida

1. Muestra el README y el diagrama.
2. Ejecuta `make verify` y enseña los tests.
3. Levanta API/worker y crea un pedido con el JSON de `samples/`.
4. Repite la misma petición para demostrar idempotencia.
5. Usa un email que contenga `decline` para mostrar compensación de inventario.
6. Abre la consola y enseña órdenes, métricas, inventario y auditoría.
7. Cierra con `infra/main.bicep`, RBAC, OIDC y los límites de producción.

No memorices nombres de archivos: entiende qué problema resuelve cada límite y
qué garantía ofrece. Si algo no fue probado en producción, dilo claramente.
