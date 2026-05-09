# NightscoutFoundation.Nocturne.Model.UpdateTrackerDefinitionRequest

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Name** | **string** |  | [optional] 
**Description** | **string** |  | [optional] 
**Category** | **TrackerCategory** |  | [optional] 
**Icon** | **string** |  | [optional] 
**TriggerEventTypes** | **List&lt;string&gt;** |  | [optional] 
**TriggerNotesContains** | **string** |  | [optional] 
**LifespanHours** | **int?** |  | [optional] 
**NotificationThresholds** | [**List&lt;CreateNotificationThresholdRequest&gt;**](CreateNotificationThresholdRequest.md) |  | [optional] 
**IsFavorite** | **bool?** |  | [optional] 
**DashboardVisibility** | **DashboardVisibility** |  | [optional] 
**Visibility** | **TrackerVisibility** |  | [optional] 
**StartEventType** | **string** | Event type to create when tracker is started (for Nightscout compatibility) | [optional] 
**CompletionEventType** | **string** | Event type to create when tracker is completed (for Nightscout compatibility) | [optional] 
**Mode** | **TrackerMode** |  | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)

