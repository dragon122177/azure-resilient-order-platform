param baseName string
param location string
param imageTag string
param bootstrapImage bool = false
param minimumReplicas int = 0
param registryLoginServer string
param apiIdentityResourceId string
param apiIdentityClientId string
param workerIdentityResourceId string
param workerIdentityClientId string
param keyVaultSqlConnectionSecretUri string
param serviceBusNamespace string
param serviceBusNamespaceName string
param serviceBusTopic string
param serviceBusSubscription string
param serviceBusAnalyticsSubscription string
param blobServiceUri string
param appInsightsConnectionString string
param logAnalyticsCustomerId string
@secure()
param logAnalyticsSharedKey string
param entraAuthority string
param entraAudience string
param tags object

var apiImage = bootstrapImage
  ? 'mcr.microsoft.com/k8se/quickstart:latest'
  : '${registryLoginServer}/ordergrid-api:${imageTag}'
var workerImage = bootstrapImage
  ? 'mcr.microsoft.com/k8se/quickstart:latest'
  : '${registryLoginServer}/ordergrid-worker:${imageTag}'
var apiPort = bootstrapImage ? 80 : 8080
resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${baseName}-cae'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: { customerId: logAnalyticsCustomerId, sharedKey: logAnalyticsSharedKey }
    }
    zoneRedundant: false
  }
}
resource api 'Microsoft.App/containerApps@2025-01-01' = {
  name: '${baseName}-api'
  location: location
  tags: tags
  identity: { type: 'UserAssigned', userAssignedIdentities: { '${apiIdentityResourceId}': {} } }
  properties: {
    environmentId: environment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        allowInsecure: false
        external: true
        targetPort: apiPort
        transport: 'auto'
        traffic: [{ latestRevision: true, weight: 100 }]
      }
      registries: [{ server: registryLoginServer, identity: apiIdentityResourceId }]
      secrets: [
        { name: 'sql-connection', keyVaultUrl: keyVaultSqlConnectionSecretUri, identity: apiIdentityResourceId }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          resources: { cpu: json('0.5'), memory: '1Gi' }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            { name: 'AZURE_CLIENT_ID', value: apiIdentityClientId }
            { name: 'Infrastructure__DatabaseConnectionString', secretRef: 'sql-connection' }
            { name: 'Infrastructure__InitializeDatabase', value: 'true' }
            { name: 'Infrastructure__ServiceBusNamespace', value: serviceBusNamespace }
            { name: 'Infrastructure__ServiceBusTopic', value: serviceBusTopic }
            { name: 'Infrastructure__ServiceBusSubscription', value: serviceBusSubscription }
            { name: 'Infrastructure__BlobServiceUri', value: blobServiceUri }
            { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
            { name: 'Authentication__Authority', value: entraAuthority }
            { name: 'Authentication__Audience', value: entraAudience }
          ]
          probes: [
            {
              type: 'Startup'
              httpGet: { path: '/health/live', port: apiPort, scheme: 'HTTP' }
              initialDelaySeconds: 3
              periodSeconds: 3
              failureThreshold: 20
              timeoutSeconds: 2
            }
            {
              type: 'Liveness'
              httpGet: { path: '/health/live', port: apiPort, scheme: 'HTTP' }
              periodSeconds: 15
              failureThreshold: 3
              timeoutSeconds: 3
            }
            {
              type: 'Readiness'
              httpGet: { path: '/health/ready', port: apiPort, scheme: 'HTTP' }
              periodSeconds: 10
              failureThreshold: 3
              timeoutSeconds: 3
            }
          ]
        }
      ]
      scale: {
        minReplicas: minimumReplicas
        maxReplicas: 5
        rules: [{ name: 'http-concurrency', http: { metadata: { concurrentRequests: '50' } } }]
      }
    }
  }
}
resource worker 'Microsoft.App/containerApps@2025-01-01' = {
  name: '${baseName}-worker'
  location: location
  tags: tags
  identity: { type: 'UserAssigned', userAssignedIdentities: { '${workerIdentityResourceId}': {} } }
  properties: {
    environmentId: environment.id
    configuration: {
      activeRevisionsMode: 'Single'
      registries: [{ server: registryLoginServer, identity: workerIdentityResourceId }]
      secrets: [
        { name: 'sql-connection', keyVaultUrl: keyVaultSqlConnectionSecretUri, identity: workerIdentityResourceId }
      ]
    }
    template: {
      containers: [
        {
          name: 'worker'
          image: workerImage
          resources: { cpu: json('0.5'), memory: '1Gi' }
          env: [
            { name: 'DOTNET_ENVIRONMENT', value: 'Production' }
            { name: 'AZURE_CLIENT_ID', value: workerIdentityClientId }
            { name: 'Infrastructure__DatabaseConnectionString', secretRef: 'sql-connection' }
            { name: 'Infrastructure__ServiceBusNamespace', value: serviceBusNamespace }
            { name: 'Infrastructure__ServiceBusTopic', value: serviceBusTopic }
            { name: 'Infrastructure__ServiceBusSubscription', value: serviceBusSubscription }
            { name: 'Infrastructure__ServiceBusAnalyticsSubscription', value: serviceBusAnalyticsSubscription }
            { name: 'Infrastructure__BlobServiceUri', value: blobServiceUri }
            { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
          ]
        }
      ]
      scale: {
        minReplicas: minimumReplicas
        maxReplicas: 10
        rules: [
          {
            name: 'orchestrator-backlog'
            custom: {
              type: 'azure-servicebus'
              identity: workerIdentityResourceId
              metadata: {
                namespace: serviceBusNamespaceName
                topicName: serviceBusTopic
                subscriptionName: serviceBusSubscription
                messageCount: '5'
              }
            }
          }
          {
            name: 'analytics-backlog'
            custom: {
              type: 'azure-servicebus'
              identity: workerIdentityResourceId
              metadata: {
                namespace: serviceBusNamespaceName
                topicName: serviceBusTopic
                subscriptionName: serviceBusAnalyticsSubscription
                messageCount: '5'
              }
            }
          }
        ]
      }
    }
  }
}
output apiFqdn string = api.properties.configuration.ingress.fqdn
output apiContainerAppName string = api.name
output workerContainerAppName string = worker.name
