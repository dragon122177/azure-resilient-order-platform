param baseName string
param location string
param tags object

resource api 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${baseName}-api-id'
  location: location
  tags: tags
}
resource worker 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${baseName}-worker-id'
  location: location
  tags: tags
}
output apiIdentityResourceId string = api.id
output apiIdentityClientId string = api.properties.clientId
output apiPrincipalId string = api.properties.principalId
output workerIdentityResourceId string = worker.id
output workerIdentityClientId string = worker.properties.clientId
output workerPrincipalId string = worker.properties.principalId
