param baseName string
param location string
param appInsightsResourceId string
param logAnalyticsResourceId string
param alertEmail string = ''
param tags object
var notify = !empty(alertEmail)
resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = if (notify) {
  name: '${baseName}-oncall'
  location: 'global'
  tags: tags
  properties: {
    groupShortName: 'ordergrid'
    enabled: true
    emailReceivers: [{ name: 'portfolio-owner', emailAddress: alertEmail, useCommonAlertSchema: true }]
  }
}
resource failedRequests 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${baseName}-failed-requests'
  location: 'global'
  tags: tags
  properties: {
    description: 'Detects a sustained increase in failed API requests.'
    severity: 2
    enabled: true
    scopes: [appInsightsResourceId]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    autoMitigate: true
    targetResourceType: 'Microsoft.Insights/components'
    targetResourceRegion: location
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'FailedRequestCount'
          metricNamespace: 'microsoft.insights/components'
          metricName: 'requests/failed'
          operator: 'GreaterThan'
          threshold: 5
          timeAggregation: 'Total'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: notify ? [{ actionGroupId: actionGroup.id }] : []
  }
}
resource errorBurst 'Microsoft.Insights/scheduledQueryRules@2023-12-01' = {
  name: '${baseName}-error-burst'
  location: location
  tags: tags
  properties: {
    displayName: 'OrderGrid error burst'
    enabled: true
    evaluationFrequency: 'PT5M'
    scopes: [logAnalyticsResourceId]
    severity: 2
    windowSize: 'PT10M'
    criteria: {
      allOf: [
        {
          query: 'AppTraces | where SeverityLevel >= 3 | summarize ErrorCount=count() by bin(TimeGenerated, 5m)'
          timeAggregation: 'Count'
          metricMeasureColumn: 'ErrorCount'
          operator: 'GreaterThan'
          threshold: 10
          failingPeriods: { minFailingPeriodsToAlert: 1, numberOfEvaluationPeriods: 1 }
        }
      ]
    }
    autoMitigate: true
    actions: {
      actionGroups: notify ? [actionGroup.id] : []
      customProperties: { service: 'ordergrid', runbook: 'docs/RUNBOOKS.md' }
    }
  }
}
