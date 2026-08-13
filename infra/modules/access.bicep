param apiPrincipalId string
param workerPrincipalId string
param registryName string
param serviceBusNamespaceName string
param storageAccountName string
param keyVaultName string

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = { name: registryName }
resource bus 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = { name: serviceBusNamespaceName }
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = { name: storageAccountName }
resource vault 'Microsoft.KeyVault/vaults@2023-07-01' existing = { name: keyVaultName }
var acrPull = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
var busSender = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
)
var busReceiver = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
)
var blobContributor = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
)
var secretsUser = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4633458b-17de-408a-b874-0445c86b69e6'
)

resource apiAcr 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, apiPrincipalId, acrPull)
  scope: registry
  properties: { roleDefinitionId: acrPull, principalId: apiPrincipalId, principalType: 'ServicePrincipal' }
}
resource workerAcr 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, workerPrincipalId, acrPull)
  scope: registry
  properties: { roleDefinitionId: acrPull, principalId: workerPrincipalId, principalType: 'ServicePrincipal' }
}
resource sender 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(bus.id, workerPrincipalId, busSender)
  scope: bus
  properties: { roleDefinitionId: busSender, principalId: workerPrincipalId, principalType: 'ServicePrincipal' }
}
resource receiver 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(bus.id, workerPrincipalId, busReceiver)
  scope: bus
  properties: { roleDefinitionId: busReceiver, principalId: workerPrincipalId, principalType: 'ServicePrincipal' }
}
resource blob 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, workerPrincipalId, blobContributor)
  scope: storage
  properties: { roleDefinitionId: blobContributor, principalId: workerPrincipalId, principalType: 'ServicePrincipal' }
}
resource apiVault 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(vault.id, apiPrincipalId, secretsUser)
  scope: vault
  properties: { roleDefinitionId: secretsUser, principalId: apiPrincipalId, principalType: 'ServicePrincipal' }
}
resource workerVault 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(vault.id, workerPrincipalId, secretsUser)
  scope: vault
  properties: { roleDefinitionId: secretsUser, principalId: workerPrincipalId, principalType: 'ServicePrincipal' }
}
