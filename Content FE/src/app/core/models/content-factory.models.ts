export type TopSectionId = 'dashboard' | 'agents' | 'memory' | 'scheduler' | 'settings' | 'drive';
export type ThemeMode = 'light' | 'dark';
export type SideTabId = string;

export type TopSectionItem = {
  id: TopSectionId;
  label: string;
  icon: string;
  description: string;
};

export type SideNavTemplate = {
  id: SideTabId;
  label: string;
  icon: string;
  badge: string;
  agentKey?: string;
};

export type SideNavItem = SideNavTemplate & {
  badgeValue: string | number;
};

export type UsagePoint = {
  capturedAt: string;
  requestCount: number;
  tokensIn: number;
  tokensOut: number;
  costUsd: number;
  durationMs: number;
};

export type UsageSeries = {
  agentKey: string;
  agentName: string;
  accentColor: string;
  points: UsagePoint[];
};

export type MemoryCounts = {
  globalApproved: number;
  localApproved: number;
  pendingReview: number;
};

export type VideoItem = {
  id: string;
  title: string;
  topic: string;
  format: string;
  stage: string;
  storageFolder: string;
  driveFileId?: string;
  sourceAgentKey: string;
  platforms: string[];
  createdAt: string;
  publishedAt?: string;
};

export type PlatformPublicationWidget = {
  platform: string;
  publishedCount: number;
  scheduledCount: number;
  failedCount: number;
  totalViews: number;
};

export type AgentRun = {
  id: string;
  agentKey: string;
  title: string;
  status: string;
  summary: string;
  queuedAt: string;
  completedAt?: string;
};

export type AgentSummary = {
  key: string;
  name: string;
  description: string;
  icon: string;
  category: string;
  requiresConnection: boolean;
  supportsOpenRouter: boolean;
  isConnected: boolean;
  providerName: string;
  modelName: string;
  status: string;
  capabilitySummary: string;
  lastRunAt?: string;
  localMemoryHighlights: string[];
  recentRuns: AgentRun[];
};

export type ChatMessage = {
  id: string;
  agentKey: string;
  role: string;
  content: string;
  createdAt: string;
};

export type MemoryRecord = {
  id: string;
  scope: string;
  agentKey?: string;
  title: string;
  content: string;
  status: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
  approvedAt?: string;
};

export type ScheduleJob = {
  id: string;
  name: string;
  type: string;
  agentKey?: string;
  isEnabled: boolean;
  status: string;
  trigger: string;
  queueMode: string;
  nextRunAt?: string;
  lastRunAt?: string;
  queuedAt?: string;
  startedAt?: string;
  notes: string;
};

export type DriveMetadata = {
  storageFolderName: string;
  storageFolderPath: string;
  storageFolderUrl: string;
};

export type AgentSettings = {
  agentKey: string;
  name: string;
  description: string;
  icon: string;
  category: string;
  requiresConnection: boolean;
  supportsOpenRouter: boolean;
  isConnected: boolean;
  providerName: string;
  modelName: string;
  baseUrl: string;
  apiKey: string;
  clientId: string;
  clientSecret: string;
  refreshToken: string;
  sourceVideoPath: string;
  sourceVideoFolderId: string;
  sourceVideoFolderName: string;
  storageFolderId: string;
  storageFolderName: string;
  storageFolderPath: string;
  storageFolderUrl: string;
  useOpenRouter: boolean;
  openRouterModel: string;
  openRouterApiKey: string;
  notes: string;
  updatedAt: string;
};

export type MemorySuggestionDto = {
  id: string;
  scope: string;
  agentName?: string;
  content: string;
  reason: string;
  status: string;
  createdAt: string;
};

export type DriveSettings = {
  clientId: string;
  clientSecret: string;
  refreshToken: string;
  rootFolderId: string;
  isConnected: boolean;
  connectedAccount?: string;
  storageUsed?: number;
  storageAvailable?: number;
  pollingInterval?: number;
  autoCreateFolders?: boolean;
};

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
  pipelineStatus?: string;
};


export type ProviderOption = {
  category: string;
  providers: string[];
};

export type ProviderSelectionKey = 'textProvider' | 'videoProvider' | 'uploadProvider' | 'storageProvider';

export type ProviderSelections = Record<ProviderSelectionKey, string>;

export type ProviderSelectOptions = Record<ProviderSelectionKey, string[]>;

export type ProviderRequirementField = {
  id: string;
  label: string;
  inputType: 'text' | 'password' | 'url';
  helpText: string;
};

export type ProviderRequirement = {
  providerName: string;
  displayName: string;
  providerType: string;
  documentationUrl?: string;
  notes: string;
  fields: ProviderRequirementField[];
};

export type LegacyVideoTaskDraft = {
  topic: string;
  platform: string;
  format: string;
  audience: string;
  goal: string;
  autoSaveLocalMemory: boolean;
};

export type LegacyVideoTaskRunResult = {
  agentName: string;
  summary: string;
};

export type LegacyVideoTaskRun = {
  topic: string;
  platform: string;
  format: string;
  audience: string;
  goal: string;
  agentResults: LegacyVideoTaskRunResult[];
  createdAt: string;
};

export type ReadyVideoListItem = {
  id: string;
  topic: string;
  platform: string;
  format: string;
};

export type DashboardWorkspace = {
  usageSeries: UsageSeries[];
  memoryCounts: MemoryCounts;
  readyVideos: VideoItem[];
  backlogVideos: VideoItem[];
  publishedWidgets: PlatformPublicationWidget[];
  recentlyPublished: VideoItem[];
  recentRuns: AgentRun[];
};

export type AgentWorkspace = {
  agents: AgentSummary[];
  chatMessages: ChatMessage[];
};

export type MemoryWorkspace = {
  counts: MemoryCounts;
  reviewQueue: MemoryRecord[];
  globalMemories: MemoryRecord[];
  localMemories: MemoryRecord[];
};

export type SchedulerWorkspace = {
  manualSchedules: ScheduleJob[];
  dailyPostingJobs: ScheduleJob[];
  retryJobs: ScheduleJob[];
  queueJobs: ScheduleJob[];
};

export type SettingsWorkspace = {
  agents: AgentSettings[];
  providerOptions: ProviderOption[];
};

export type WorkspaceBootstrap = {
  dashboard: DashboardWorkspace;
  agents: AgentWorkspace;
  memory: MemoryWorkspace;
  scheduler: SchedulerWorkspace;
  settings: SettingsWorkspace;
  drive: DriveSettings;
  generatedAt: string;
};

export type AgentChatResponse = {
  blocked: boolean;
  message: string;
  messages: ChatMessage[];
};

export type AgentStreamChunk = {
  type: 'thought' | 'delta' | 'tool' | 'done';
  content: string;
  message?: ChatMessage;
};

export type SaveAgentSettingsRequest = {
  providerName: string;
  modelName: string;
  baseUrl: string;
  apiKey: string;
  clientId: string;
  clientSecret: string;
  refreshToken: string;
  sourceVideoPath: string;
  sourceVideoFolderId: string;
  sourceVideoFolderName: string;
  storageFolderId: string;
  storageFolderName: string;
  storageFolderPath: string;
  storageFolderUrl: string;
  useOpenRouter: boolean;
  openRouterModel: string;
  openRouterApiKey: string;
  notes: string;
};

export type ManualScheduleDraft = {
  name: string;
  agentKey: string;
  trigger: string;
  notes?: string;
  isEnabled: boolean;
};

export const TOP_SECTION_ITEMS: TopSectionItem[] = [
  { id: 'dashboard', label: 'Dashboard', icon: 'fa-chart-line', description: 'Studio overview and delivery pipeline' },
  { id: 'agents', label: 'Agents', icon: 'fa-robot', description: 'Main Brain plus all creation and publishing agents' },
  { id: 'memory', label: 'Memory', icon: 'fa-brain', description: 'Global memory, local memory, and review workflow' },
  { id: 'scheduler', label: 'Scheduler', icon: 'fa-calendar-check', description: 'Manual and automated execution control' },
  { id: 'drive', label: 'Drive', icon: 'fa-google-drive', description: 'Global Google Drive storage and configuration' },
  { id: 'settings', label: 'Settings', icon: 'fa-sliders', description: 'Per-agent API, model, and storage configuration' }
];

export const AGENT_NAV_ITEMS: SideNavTemplate[] = [
  { id: 'main-brain', label: 'Main Brain', icon: 'fa-brain', badge: 'agentConnection', agentKey: 'main-brain' },
  { id: 'trend-agent', label: 'Trend Agent', icon: 'fa-chart-simple', badge: 'agentConnection', agentKey: 'trend-agent' },
  { id: 'script-agent', label: 'Script Agent', icon: 'fa-pen-nib', badge: 'agentConnection', agentKey: 'script-agent' },
  { id: 'video-generation-agent', label: 'Video Generator', icon: 'fa-film', badge: 'agentConnection', agentKey: 'video-generation-agent' },
  { id: 'shorts-agent-1', label: 'Shorts Agent 1', icon: 'fa-bolt', badge: 'agentConnection', agentKey: 'shorts-agent-1' },
  { id: 'shorts-agent-2', label: 'Shorts Agent 2', icon: 'fa-wand-magic-sparkles', badge: 'agentConnection', agentKey: 'shorts-agent-2' },
  { id: 'youtube-agent', label: 'YouTube Agent', icon: 'fa-youtube', badge: 'agentConnection', agentKey: 'youtube-agent' },
  { id: 'tiktok-agent', label: 'TikTok Agent', icon: 'fa-music', badge: 'agentConnection', agentKey: 'tiktok-agent' },
  { id: 'instagram-agent', label: 'Instagram Agent', icon: 'fa-instagram', badge: 'agentConnection', agentKey: 'instagram-agent' },
  { id: 'facebook-agent', label: 'Facebook Agent', icon: 'fa-facebook', badge: 'agentConnection', agentKey: 'facebook-agent' },
  { id: 'linkedin-agent', label: 'LinkedIn Agent', icon: 'fa-linkedin', badge: 'agentConnection', agentKey: 'linkedin-agent' }
];

export const SETTINGS_NAV_ITEMS: SideNavTemplate[] = [
  { id: 'settings-unified', label: 'Unified Settings', icon: 'fa-sliders', badge: 'settingsStatus' }
];

export const MENU_BY_SECTION: Record<TopSectionId, SideNavTemplate[]> = {
  dashboard: [
    { id: 'dashboard-overview', label: 'Overview', icon: 'fa-gauge-high', badge: 'overview' },
    { id: 'dashboard-usage', label: 'API Usage', icon: 'fa-chart-column', badge: 'usageCount' },
    { id: 'dashboard-memory', label: 'Memory Signals', icon: 'fa-memory', badge: 'pendingMemory' },
    { id: 'dashboard-drive', label: 'Drive Pipeline', icon: 'fa-google-drive', badge: 'readyCount' },
    { id: 'dashboard-published', label: 'Published', icon: 'fa-earth-americas', badge: 'publishedCount' },
    { id: 'dashboard-growth', label: 'Month 2+', icon: 'fa-seedling', badge: 'future' }
  ],
  agents: AGENT_NAV_ITEMS,
  memory: [
    { id: 'memory-global', label: 'Global Memory', icon: 'fa-globe', badge: 'globalMemoryCount' },
    { id: 'memory-local', label: 'Local Memory', icon: 'fa-layer-group', badge: 'localMemoryCount' },
    { id: 'memory-review', label: 'Review Queue', icon: 'fa-clipboard-check', badge: 'pendingMemory' }
  ],
  scheduler: [
    { id: 'scheduler-manual', label: 'Manual Scheduler', icon: 'fa-calendar-plus', badge: 'manualScheduleCount' },
    { id: 'scheduler-daily', label: 'Daily Posting', icon: 'fa-calendar-day', badge: 'dailyScheduleCount' },
    { id: 'scheduler-retry', label: 'Retry Uploads', icon: 'fa-rotate-right', badge: 'retryScheduleCount' },
    { id: 'scheduler-queue', label: 'Queue Execution', icon: 'fa-list-check', badge: 'queueScheduleCount' }
  ],
  settings: SETTINGS_NAV_ITEMS,
  drive: [
    { id: 'drive-explorer', label: 'Explorer', icon: 'fa-folder-open', badge: 'driveFiles' },
    { id: 'drive-config', label: 'Configuration', icon: 'fa-gear', badge: 'driveStatus' }
  ]
};

export const INITIAL_MANUAL_SCHEDULE: ManualScheduleDraft = {
  name: 'Manual content sync',
  agentKey: 'trend-agent',
  trigger: 'Today 7:00 PM',
  notes: 'Create a manual scheduler entry for any custom task handoff.',
  isEnabled: true
};

export const INITIAL_VIDEO_TASK: LegacyVideoTaskDraft = {
  topic: '',
  platform: 'youtube',
  format: 'short',
  audience: '',
  goal: '',
  autoSaveLocalMemory: true
};

export const PROVIDER_SELECT_OPTIONS: ProviderSelectOptions = {
  textProvider: ['OpenAI', 'Claude', 'Gemini', 'OpenRouter'],
  videoProvider: ['Runway', 'Pika', 'Luma', 'Manual'],
  uploadProvider: ['YouTube', 'TikTok', 'Instagram', 'Facebook', 'LinkedIn', 'DryRun'],
  storageProvider: ['Google Drive', 'Local Storage', 'Manual']
};

export type ErrorLog = {
  id: string;
  agentKey: string;
  message: string;
  timestamp: string;
  severity?: string;
};

export type PipelineStageType = 'TrendAnalysis' | 'ScriptGeneration' | 'AssetGathering' | 'Editing' | 'ShortsGeneration' | 'Publishing';

export type VideoPipelineJobStage = {
  stageType: PipelineStageType | string;
  status: string;
  progress?: number;
};

export type VideoPipelineJob = {
  id: string;
  fileName: string;
  agentKey?: string;
  status: string;
  currentStage: string;
  currentProgress: number;
  retryCount?: number;
  stages: VideoPipelineJobStage[];
};



export type ViralPattern = {
  id: string;
  name: string;
  description: string;
  confidence: number;
  sourcePlatform: string;
  discoveredAt: string;
};

export type VideoAnalytics = {
  videoId: string;
  platform: string;
  views: number;
  likes: number;
  shares: number;
  comments: number;
  watchTimeSeconds: number;
  ctr: number;
  collectedAt: string;
};

export type AgentWorkspaceState = {
  agentKey: string;
  status: 'idle' | 'running' | 'error' | 'disabled';
  isConnected: boolean;
  modelName: string;
  localMemory: Record<string, any>;
  recentRuns: AgentRun[];
  activeJobId: string | null;
  chatMessages: ChatMessage[];
  errorLog: ErrorLog[];
  lastRunAt: string | null;
};

export type GlobalMemoryFull = {
  version: string;
  lastUpdated: string;
  folderRegistry: Record<string, string>;
  videoConstraints: {
    shorts: Record<string, any>;
    longForm: Record<string, any>;
  };
  trendConfig: {
    tier1Sites: string[];
    tier2Sites: string[];
    tier3Sites: string[];
    useOpenRouterFallback: boolean;
  };
  scheduleSlots: string[];
  agentStatuses: Record<string, string>;
};

export type AgentLocalMemory = {
  agentKey: string;
  lastRunAt: string | null;
  totalRuns: number;
  successes: number;
  failures: number;
  config: Record<string, any>;
  configHistory: any[];
};
