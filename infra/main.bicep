targetScope = 'resourceGroup'
@minLength(3)
@maxLength(12)
param namePrefix string = 'ordergrid'
param location string = 'japaneast'
@allowed(['dev', 'staging', 'prod'])
param environmentName string = 'dev'
param imageTag string = 'latest'
param bootstrapImage bool = false
param sqlAdministratorLogin string = 'ordergridadmin'
@secure()
param sqlAdministratorPassword string
param tenantId string = tenant().tenantId
param entraAuthority string = '${environment().authentication.loginEndpoint}${tenantId}/v2.0'
param entraAudience string = 'api://ordergrid'
param alertEmail string = ''
param enableFunctionsSubscription bool = false

var suffix = toLower(uniqueString(subscription().subscriptionId, resourceGroup().id, environmentName))
var baseName = '${namePrefix}-${environmentName}'
var compactName = take(replace('${namePrefix}${environmentName}${suffix}', '-', ''), 20)
var tags = {
  application: 'ordergrid'
  environment: environmentName
  managedBy: 'bicep'
  workload: 'order-orchestration'
}
module identity 'modules/identity.bicep' = {
  name: 'identity'
  params: { baseName: baseName, location: location, tags: tags }
}
module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: { baseName: baseName, location: location, tags: tags }
}
module registry 'modules/registry.bicep' = {
  name: 'registry'
  params: { registryName: '${compactName}acr', location: location, tags: tags }
}
module messaging 'modules/messaging.bicep' = {
  name: 'messaging'
  params: {
    namespaceName: '${compactName}sb'
    location: location
    enableFunctionsSubscription: enableFunctionsSubscription
    tags: tags
  }
}
module data 'modules/data.bicep' = {
  name: 'data'
  params: {
    compactName: compactName
    location: location
    tenantId: tenantId
    sqlAdministratorLogin: sqlAdministratorLogin
    sqlAdministratorPassword: sqlAdministratorPassword
    tags: tags
  }
}
module access 'modules/access.bicep' = {
  name: 'access'
  params: {
    apiPrincipalId: identity.outputs.apiPrincipalId
    workerPrincipalId: identity.outputs.workerPrincipalId
    registryName: registry.outputs.registryName
    serviceBusNamespaceName: messaging.outputs.namespaceName
    storageAccountName: data.outputs.storageAccountName
    keyVaultName: data.outputs.keyVaultName
  }
}
module compute 'modules/compute.bicep' = {
  name: 'compute'
  params: {
    baseName: baseName
    location: location
    imageTag: imageTag
    bootstrapImage: bootstrapImage
    minimumReplicas: environmentName == 'prod' ? 1 : 0
    registryLoginServer: registry.outputs.loginServer
    apiIdentityResourceId: identity.outputs.apiIdentityResourceId
    apiIdentityClientId: identity.outputs.apiIdentityClientId
    workerIdentityResourceId: identity.outputs.workerIdentityResourceId
    workerIdentityClientId: identity.outputs.workerIdentityClientId
    keyVaultSqlConnectionSecretUri: data.outputs.sqlConnectionSecretUri
    serviceBusNamespace: messaging.outputs.fullyQualifiedNamespace
    serviceBusNamespaceName: messaging.outputs.namespaceName
    serviceBusTopic: messaging.outputs.topicName
    serviceBusSubscription: messaging.outputs.orchestratorSubscriptionName
    serviceBusAnalyticsSubscription: messaging.outputs.analyticsSubscriptionName
    blobServiceUri: data.outputs.blobServiceUri
    appInsightsConnectionString: monitoring.outputs.connectionString
    logAnalyticsCustomerId: monitoring.outputs.logAnalyticsCustomerId
    logAnalyticsSharedKey: monitoring.outputs.logAnalyticsSharedKey
    entraAuthority: entraAuthority
    entraAudience: entraAudience
    tags: tags
  }
  dependsOn: [access]
}
module alerts 'modules/alerts.bicep' = {
  name: 'alerts'
  params: {
    baseName: baseName
    location: location
    appInsightsResourceId: monitoring.outputs.appInsightsResourceId
    logAnalyticsResourceId: monitoring.outputs.logAnalyticsResourceId
    alertEmail: alertEmail
    tags: tags
  }
}
output apiUrl string = 'https://${compute.outputs.apiFqdn}'
output apiContainerAppName string = compute.outputs.apiContainerAppName
output workerContainerAppName string = compute.outputs.workerContainerAppName
output registryName string = registry.outputs.registryName
output serviceBusNamespace string = messaging.outputs.namespaceName
output keyVaultName string = data.outputs.keyVaultName
output sqlServerName string = data.outputs.sqlServerName
output sqlDatabaseName string = data.outputs.sqlDatabaseName
