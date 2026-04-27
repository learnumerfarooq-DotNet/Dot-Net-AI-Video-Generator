# 20 — Angular Drive Explorer & Settings

## Purpose
Build the Google Drive integration UI (OAuth flow, folder explorer, agent-to-folder mapping) and the unified Settings page (per-agent API keys, model configuration, provider selection).

---

## PART A: DRIVE INTEGRATION

### Existing Components
- `drive-config.ts` — Drive OAuth configuration (enhance)
- `drive-explorer.ts` — File/folder browser (enhance)
- `drive-mapping.ts` — Agent-to-folder mapping (enhance)
- `drive-oauth-callback.ts` — OAuth callback handler (working)

---

### COMPONENT: drive-explorer (Enhanced) — ~150 lines HTML

```
├── Breadcrumb Navigation: Home / RAW / scripts /
├── Toolbar:
│   ├── [New Folder] [Upload File] [Refresh] [View: Grid|List]
│   ├── Search input
│   └── Sort: Name | Modified | Size
├── File/Folder Grid:
│   Each item:
│   ├── Icon (folder/video/json/image)
│   ├── Name
│   ├── Modified date
│   ├── Size (formatted)
│   └── Actions: [Open] [Download] [Delete] [Move]
├── Video Preview Panel (when video selected):
│   ├── Thumbnail
│   ├── Duration, Resolution, Size
│   ├── Pipeline Status (if tracked)
│   └── [Start Pipeline] button (if in /RAW/)
└── Folder Stats: Total Files, Total Size, Free Space
```

### TypeScript — ~100 lines
```typescript
currentFolderId = signal<string>('root');
currentPath = signal<string[]>(['Home']);
files = signal<DriveFile[]>([]);
selectedFile = signal<DriveFile | null>(null);
viewMode = signal<'grid' | 'list'>('grid');
sortBy = signal<'name' | 'modified' | 'size'>('modified');

navigateToFolder(folderId: string, name: string);
goBack();
refreshFiles();
createFolder(name: string);
uploadFile(file: File);
deleteFile(fileId: string);
moveFile(fileId: string, targetFolderId: string);
startPipeline(fileId: string, fileName: string);
downloadFile(fileId: string);
```

### New Models
```typescript
export type DriveFile = {
    id: string;
    name: string;
    mimeType: string;
    size: number;
    modifiedTime: string;
    parents: string[];
    webViewLink: string;
    iconLink: string;
    thumbnailLink?: string;
    isFolder: boolean;
    pipelineJobId?: string;
    pipelineStatus?: PipelineStatus;
};
```

---

### COMPONENT: drive-mapping (Enhanced) — ~120 lines HTML

```
├── Folder Mapping Table:
│   ├── Columns: Agent Name | Agent Icon | Drive Path | Folder ID | Status | Actions
│   ├── 10 mappings (from Global Memory FolderRegistry)
│   ├── Status: ✓ Exists / ✗ Missing / ⚠ Empty
│   └── Actions: [Browse] [Create Folder] [Change Path]
├── Auto-Setup Button: "Create All Missing Folders"
├── Validation Panel:
│   ├── Green: All folders exist and accessible
│   ├── Yellow: Some folders empty
│   └── Red: Missing folders or no Drive access
└── Folder Tree Visualization (collapsible tree)
```

---

### COMPONENT: drive-config (Enhanced) — ~80 lines HTML

```
├── Connection Status Card:
│   ├── Status: Connected ✓ / Disconnected ✗
│   ├── Connected Account: user@gmail.com
│   ├── Root Folder ID
│   └── Storage Used / Available
├── OAuth Section:
│   ├── Client ID input (masked)
│   ├── Client Secret input (masked)
│   ├── [Connect Google Drive] button → OAuth flow
│   ├── [Disconnect] button
│   └── [Test Connection] button
├── Advanced:
│   ├── Root Folder ID override
│   ├── Polling Interval (seconds)
│   └── Auto-create folders toggle
```

---

## PART B: SETTINGS UI

### Existing Component
- `settings-main/` — Unified settings page (enhance significantly)

---

### COMPONENT: settings-main (Enhanced) — ~200 lines HTML

```
├── Agent Selector: Tabs or sidebar with all 11 agents
├── Selected Agent Settings Panel:
│   ├── General Section:
│   │   ├── Agent Name (read-only)
│   │   ├── Category (Creation/Publishing/Analysis)
│   │   ├── Connection Status badge
│   │   └── Enabled toggle
│   ├── AI Model Section:
│   │   ├── Provider dropdown: OpenRouter / Gemini / OpenAI / Claude
│   │   ├── Model Name (auto-populated based on provider)
│   │   ├── API Key input (masked, with [Show] toggle)
│   │   ├── Base URL input
│   │   ├── Temperature slider (0.0 - 1.0)
│   │   └── Max Tokens input
│   ├── Platform Connection Section (for upload agents):
│   │   ├── OAuth Status + [Connect] button
│   │   ├── Client ID / Client Secret inputs
│   │   ├── Refresh Token (read-only, from OAuth)
│   │   └── Channel/Account ID (read-only)
│   ├── Storage Section:
│   │   ├── Input Folder Path (from folder registry)
│   │   ├── Output Folder Path
│   │   ├── Folder Browser [Browse] button
│   │   └── Storage Folder ID
│   ├── OpenRouter Section:
│   │   ├── Use OpenRouter toggle
│   │   ├── OpenRouter API Key
│   │   ├── Model selector dropdown (free models)
│   │   └── [Test Connection] button
│   └── Actions:
│       ├── [Save Settings] button
│       ├── [Reset to Defaults] button
│       └── [Test Agent] button → runs a quick test
├── Global Settings Panel:
│   ├── OpenRouter Master API Key
│   ├── Default Model
│   ├── FFmpeg Path
│   ├── Temp Storage Path
│   └── Database Connection (read-only)
```

---

### SETTINGS STORE ENHANCEMENTS

**File**: `features/settings/store/settings.store.ts`
**New methods**: 6

```typescript
loadAgentSettings(agentKey: string);
saveAgentSettings(agentKey: string, settings: SaveAgentSettingsRequest);
resetAgentSettings(agentKey: string);
testAgentConnection(agentKey: string);
loadGlobalSettings();
saveGlobalSettings(settings: any);
```

---

## DRIVE STORE ENHANCEMENTS

**File**: `features/drive/store/drive.store.ts`
**New methods**: 10

```typescript
loadFiles(folderId: string);
navigateToFolder(folderId: string);
createFolder(parentId: string, name: string);
uploadFile(folderId: string, file: File);
deleteFile(fileId: string);
moveFile(fileId: string, targetId: string);
startPipeline(fileId: string, fileName: string);
loadFolderMapping();
createMissingFolders();
testConnection();
```

---

## BACKEND API ENDPOINTS

### Drive APIs (existing + new)
```
GET    /api/drive/files/{folderId}           → List files (existing)
POST   /api/drive/folder                    → Create folder
POST   /api/drive/upload                    → Upload file
DELETE /api/drive/file/{fileId}             → Delete file
POST   /api/drive/file/{fileId}/move        → Move file
GET    /api/drive/mapping                   → Get folder mapping
POST   /api/drive/mapping/create-missing    → Create all missing folders
POST   /api/drive/test                      → Test connection
GET    /api/drive/storage-info              → Storage used/available
POST   /api/drive/pipeline/start            → Start pipeline from file
```

### Settings APIs (existing + new)
```
GET    /api/settings/agents                  → Get all agent settings
GET    /api/settings/agents/{key}            → Get agent settings
PUT    /api/settings/agents/{key}            → Save agent settings (existing)
POST   /api/settings/agents/{key}/reset      → Reset to defaults
POST   /api/settings/agents/{key}/test       → Test agent connection
GET    /api/settings/global                  → Global settings
PUT    /api/settings/global                  → Save global settings
```

## ESTIMATED TIME: 6-8 hours (Drive: 3-4h, Settings: 3-4h)
