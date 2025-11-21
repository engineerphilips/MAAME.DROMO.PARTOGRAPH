# Offline Sync Capabilities

This document describes the offline synchronization features implemented in the MAAME.DROMO.PARTOGRAPH application.

## Overview

The partograph application now supports full offline operation with automatic synchronization when network connectivity is available. Users can record patient data, partograph measurements, and vital signs while offline, and all changes will be synchronized with the server when the device reconnects.

## Features

### 1. **Offline-First Architecture**
- All data is stored locally in SQLite database
- Application works fully offline without network connectivity
- Data is automatically tracked for synchronization

### 2. **Automatic Background Sync**
- Configurable automatic synchronization at regular intervals
- Sync triggers automatically when network connectivity is restored
- Efficient sync of only changed data (delta sync)

### 3. **Conflict Resolution**
- Detects conflicts when the same record is modified on multiple devices
- Version-based conflict detection using timestamps and version numbers
- Hash-based change detection to verify data integrity
- Manual conflict resolution UI for user decisions

### 4. **Network Connectivity Monitoring**
- Real-time monitoring of network status
- Automatic retry with exponential backoff on network failures
- Connection testing to verify server availability

### 5. **Sync Status Tracking**
- Real-time sync progress indicators
- Pending changes counter
- Conflict counter
- Last sync timestamp

## Architecture

### Core Components

#### 1. **ConnectivityService** (`IConnectivityService`)
Monitors network connectivity and provides:
- Real-time connection status
- Connectivity change events
- Host reachability testing

**Location:** `Services/ConnectivityService.cs`

#### 2. **SyncApiClient** (`ISyncApiClient`)
HTTP client for server communication with:
- Automatic retry logic with exponential backoff (3 retries)
- JSON serialization/deserialization
- Separate endpoints for different entity types
- Connection health testing

**Location:** `Services/SyncApiClient.cs`

#### 3. **SyncService** (`ISyncService`)
Core synchronization orchestrator that:
- Manages push/pull operations
- Tracks sync status and progress
- Handles conflict detection
- Coordinates between API client and local repositories

**Location:** `Services/SyncService.cs`

#### 4. **BackgroundSyncService**
Automatic background synchronization service:
- Configurable sync intervals (5-120 minutes)
- Automatic sync on connectivity restore
- Can be enabled/disabled by user

**Location:** `Services/BackgroundSyncService.cs`

### Data Models

#### Sync Metadata (Per Record)
Every synced entity includes the following fields:
- `CreatedTime` - Unix timestamp of creation
- `UpdatedTime` - Unix timestamp of last update
- `DeletedTime` - Unix timestamp of deletion (soft delete)
- `DeviceId` - Device that created/updated the record
- `OriginDeviceId` - Device where record was originally created
- `SyncStatus` - 0=pending, 1=synced, 2=conflict
- `Version` - Local version number (incremented on each change)
- `ServerVersion` - Last known server version
- `Deleted` - Soft delete flag (0=active, 1=deleted)
- `ConflictData` - JSON of conflicting server record
- `DataHash` - SHA256 hash for change detection

#### Sync DTOs
- `SyncPullRequest` - Request to pull data from server
- `SyncPullResponse<T>` - Server response with records
- `SyncPushRequest<T>` - Request to push local changes
- `SyncPushResponse<T>` - Server response with success/conflicts/errors
- `ConflictRecord<T>` - Represents a data conflict
- `SyncError` - Represents a sync error

**Location:** `Models/SyncModels.cs`

## Usage

### Accessing Sync Settings

Navigate to the Sync Settings page:
```csharp
await Shell.Current.GoToAsync("//syncsettings");
```

### Manual Sync

Trigger a manual sync programmatically:
```csharp
var syncService = ServiceProvider.GetService<ISyncService>();
var result = await syncService.SyncAsync();

if (result.Success)
{
    Console.WriteLine($"Synced: {result.TotalPushed} sent, {result.TotalPulled} received");
}
```

### Configuring Background Sync

```csharp
var backgroundSync = ServiceProvider.GetService<BackgroundSyncService>();

// Enable/disable
backgroundSync.IsEnabled = true;

// Set interval (minutes)
backgroundSync.SyncIntervalMinutes = 30;

// Manual trigger
await backgroundSync.TriggerSyncAsync();
```

### Monitoring Sync Status

Subscribe to sync events:
```csharp
var syncService = ServiceProvider.GetService<ISyncService>();

syncService.StatusChanged += (sender, status) =>
{
    Console.WriteLine($"Sync status: {status}");
};

syncService.ProgressChanged += (sender, progress) =>
{
    Console.WriteLine($"Progress: {progress.ProgressPercentage}%");
};
```

## Configuration

### API URL Configuration

Set the sync server URL in preferences:
```csharp
Preferences.Set("SyncApiUrl", "https://your-server.com/api");
```

Or configure in the Sync Settings UI.

### Background Sync Settings

Settings are stored in preferences:
- `BackgroundSyncEnabled` - Boolean, default: true
- `SyncIntervalMinutes` - Integer, default: 15 minutes
- `LastSyncTime` - Long (ticks), timestamp of last successful sync
- `LastPullTimestamp` - Long (milliseconds), unix timestamp for incremental pull

## Sync Flow

### Push Operation
1. Query local database for records with `SyncStatus = 0` (pending)
2. Send records to server via `PushAsync` endpoint
3. Server validates and processes records
4. Server returns:
   - Success IDs - Records successfully saved
   - Conflicts - Records with version conflicts
   - Errors - Records that failed validation
5. Update local records:
   - Mark successful records as `SyncStatus = 1`
   - Mark conflicts as `SyncStatus = 2` with conflict data
   - Log errors for review

### Pull Operation
1. Get last pull timestamp from preferences
2. Request records updated since last sync via `PullAsync` endpoint
3. Server returns records with `UpdatedTime > LastPullTimestamp`
4. Upsert records into local database:
   - Insert if new
   - Update if exists and `ServerVersion >= LocalServerVersion`
   - Detect conflicts if local has unsynced changes
5. Update last pull timestamp

### Full Sync Operation
1. Run Push operation to send local changes
2. Run Pull operation to receive server changes
3. Return combined results

## Conflict Resolution

### Conflict Detection
Conflicts occur when:
- Same record modified on multiple devices
- Local record has `SyncStatus = 0` (unsynced changes)
- Server has a newer version of the record

### Conflict Resolution Options
1. **Use Local Version** - Keep local changes, mark for re-push
2. **Use Server Version** - Discard local changes, accept server data

### Resolution API
```csharp
var syncService = ServiceProvider.GetService<ISyncService>();

// Use local version
await syncService.ResolveConflictAsync(recordId, useLocalVersion: true);

// Use server version
await syncService.ResolveConflictAsync(recordId, useLocalVersion: false);
```

## Database Schema

### Sync Triggers

Automatic triggers maintain sync metadata:

**Insert Trigger:**
```sql
CREATE TRIGGER trg_patient_insert
AFTER INSERT ON Tbl_Patient
BEGIN
    UPDATE Tbl_Patient
    SET createdtime = COALESCE(NEW.createdtime, strftime('%s', 'now') * 1000),
        updatedtime = COALESCE(NEW.updatedtime, strftime('%s', 'now') * 1000)
    WHERE ID = NEW.ID;
END;
```

**Update Trigger:**
```sql
CREATE TRIGGER trg_patient_update
AFTER UPDATE ON Tbl_Patient
BEGIN
    UPDATE Tbl_Patient
    SET updatedtime = strftime('%s', 'now') * 1000,
        version = OLD.version + 1,
        syncstatus = 0  -- Mark as pending
    WHERE ID = NEW.ID;
END;
```

### Indexes

Optimized indexes for sync operations:
```sql
CREATE INDEX idx_patient_sync ON Tbl_Patient(updatedtime, syncstatus);
CREATE INDEX idx_patient_server_version ON Tbl_Patient(serverversion);
```

## Error Handling

### Retry Logic
- HTTP client retries failed requests up to 3 times
- Exponential backoff: 1s, 2s, 4s delays
- Only retries on server errors (5xx) or network issues

### Error Types
- **Network Errors** - No connectivity, timeouts
- **Server Errors** - 5xx responses, API failures
- **Validation Errors** - Invalid data, business rule violations
- **Conflict Errors** - Version mismatches

### Error Recovery
1. Network errors - Automatically retry, queue for next sync
2. Validation errors - Log locally, require user correction
3. Conflicts - Present to user for manual resolution

## Performance Considerations

### Optimizations
- **Delta Sync** - Only syncs changed records
- **Batched Operations** - Sends multiple records per request
- **Indexed Queries** - Fast lookup of pending records
- **Background Processing** - Non-blocking UI operations
- **Compression** - JSON payloads (future enhancement)

### Scalability
- Designed for thousands of records per device
- Efficient incremental sync reduces bandwidth
- Local SQLite handles millions of records

## Security Considerations

### Data Security
- HTTPS required for all API communication
- Authentication tokens (to be implemented)
- Data encryption at rest (platform-dependent)

### Conflict Data
- Stores conflicting server record as JSON
- Includes in ConflictData field for review
- Cleared after conflict resolution

## Testing

### Unit Testing
Test sync components individually:
```csharp
// Mock dependencies
var mockApiClient = new Mock<ISyncApiClient>();
var mockConnectivity = new Mock<IConnectivityService>();

// Create service
var syncService = new SyncService(
    mockApiClient.Object,
    mockConnectivity.Object,
    patientRepo,
    partographRepo,
    logger
);

// Test sync operation
var result = await syncService.SyncAsync();
Assert.True(result.Success);
```

### Integration Testing
1. Create test records offline
2. Trigger sync
3. Verify records on server
4. Modify records on server
5. Pull updates
6. Verify local database

### Conflict Testing
1. Create same record on two devices
2. Modify differently on each
3. Sync both devices
4. Verify conflict detection
5. Resolve conflict
6. Verify resolution

## Troubleshooting

### Sync Not Working
1. Check network connectivity
2. Verify API URL is correct
3. Test connection using "Test Connection" button
4. Check server logs for errors
5. Verify authentication (if implemented)

### Data Not Appearing
1. Check sync status - is sync completed?
2. Look for conflicts - may need resolution
3. Verify data meets validation rules
4. Check for errors in sync result

### High Conflict Count
1. Multiple users editing same records
2. Clock synchronization issues between devices
3. Long offline periods
4. Consider conflict resolution strategy

## Future Enhancements

### Planned Features
- [ ] Measurement-level sync (FHR, Contractions, etc.)
- [ ] Attachment/image sync
- [ ] Differential sync (only changed fields)
- [ ] Compression for large payloads
- [ ] Sync statistics and analytics
- [ ] Custom conflict resolution rules
- [ ] Sync priority queues
- [ ] Webhook notifications for sync events

### Server Requirements
The sync implementation expects a REST API with the following endpoints:

**Pull Endpoints:**
- `POST /api/sync/pull/patients` - Pull patient records
- `POST /api/sync/pull/partographs` - Pull partograph records
- `POST /api/sync/pull/{tableName}` - Pull measurement records
- `POST /api/sync/pull/staff` - Pull staff records

**Push Endpoints:**
- `POST /api/sync/push/patients` - Push patient changes
- `POST /api/sync/push/partographs` - Push partograph changes
- `POST /api/sync/push/{tableName}` - Push measurement changes

**Health Check:**
- `GET /api/sync/health` - Server health check

All endpoints should accept and return JSON using the DTOs defined in `SyncModels.cs`.

## Support

For issues or questions:
1. Check this documentation
2. Review server logs
3. Enable debug logging
4. Contact system administrator

## License

This sync implementation is part of the MAAME.DROMO.PARTOGRAPH application.
