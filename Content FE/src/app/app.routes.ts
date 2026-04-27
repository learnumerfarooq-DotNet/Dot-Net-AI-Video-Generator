import { Routes } from '@angular/router';

// Dashboard
import { DashboardOverviewComponent }  from './features/dashboard/dashboard-overview/dashboard-overview';
import { DashboardUsageComponent }     from './features/dashboard/dashboard-usage/dashboard-usage';
import { DashboardMemoryComponent }    from './features/dashboard/dashboard-memory/dashboard-memory';
import { DashboardDriveComponent }     from './features/dashboard/dashboard-drive/dashboard-drive';
import { DashboardPublishedComponent } from './features/dashboard/dashboard-published/dashboard-published';
import { DashboardGrowthComponent }    from './features/dashboard/dashboard-growth/dashboard-growth';

// Agents
import { MainBrainComponent }             from './features/agents/main-brain/main-brain';
import { TrendAgentComponent }            from './features/agents/trend-agent/trend-agent';
import { ScriptAgentComponent }           from './features/agents/script-agent/script-agent';
import { VideoGenerationAgentComponent }  from './features/agents/video-generation-agent/video-generation-agent';
import { ShortsAgent1Component }          from './features/agents/shorts-agent-1/shorts-agent-1';
import { ShortsAgent2Component }          from './features/agents/shorts-agent-2/shorts-agent-2';
import { YoutubeAgentComponent }          from './features/agents/youtube-agent/youtube-agent';
import { TiktokAgentComponent }           from './features/agents/tiktok-agent/tiktok-agent';
import { InstagramAgentComponent }        from './features/agents/instagram-agent/instagram-agent';
import { FacebookAgentComponent }         from './features/agents/facebook-agent/facebook-agent';
import { LinkedinAgentComponent }         from './features/agents/linkedin-agent/linkedin-agent';

// Memory
import { MemoryGlobalComponent } from './features/memory/memory-global/memory-global';
import { MemoryLocalComponent }  from './features/memory/memory-local/memory-local';
import { MemoryReviewComponent } from './features/memory/memory-review/memory-review';

// Scheduler
import { SchedulerManualComponent } from './features/scheduler/scheduler-manual/scheduler-manual';
import { SchedulerDailyComponent }  from './features/scheduler/scheduler-daily/scheduler-daily';
import { SchedulerRetryComponent }  from './features/scheduler/scheduler-retry/scheduler-retry';
import { SchedulerQueueComponent }  from './features/scheduler/scheduler-queue/scheduler-queue';

// Drive
import { DriveExplorerComponent } from './features/drive/drive-explorer';
import { DriveMappingComponent }  from './features/drive/drive-mapping';
import { DriveOAuthCallbackComponent } from './features/drive/drive-oauth-callback';
import { DriveConfigComponent } from './features/drive/drive-config';

// Settings
import { SettingsMainComponent } from './features/settings/settings-main/settings-main';

export const routes: Routes = [
  // Default: redirect to dashboard overview
  { path: '', redirectTo: 'dashboard/overview', pathMatch: 'full' },

  // Dashboard
  { path: 'dashboard/overview',   component: DashboardOverviewComponent  },
  { path: 'dashboard/usage',      component: DashboardUsageComponent     },
  { path: 'dashboard/memory',     component: DashboardMemoryComponent    },
  { path: 'dashboard/drive',      component: DashboardDriveComponent     },
  { path: 'dashboard/published',  component: DashboardPublishedComponent },
  { path: 'dashboard/growth',     component: DashboardGrowthComponent    },

  // Agents
  { path: 'agents/main-brain',             component: MainBrainComponent             },
  { path: 'agents/trend-agent',            component: TrendAgentComponent            },
  { path: 'agents/script-agent',           component: ScriptAgentComponent           },
  { path: 'agents/video-generation-agent', component: VideoGenerationAgentComponent  },
  { path: 'agents/shorts-agent-1',         component: ShortsAgent1Component          },
  { path: 'agents/shorts-agent-2',         component: ShortsAgent2Component          },
  { path: 'agents/youtube-agent',          component: YoutubeAgentComponent          },
  { path: 'agents/tiktok-agent',           component: TiktokAgentComponent           },
  { path: 'agents/instagram-agent',        component: InstagramAgentComponent        },
  { path: 'agents/facebook-agent',         component: FacebookAgentComponent         },
  { path: 'agents/linkedin-agent',         component: LinkedinAgentComponent         },

  // Memory
  { path: 'memory/global',  component: MemoryGlobalComponent },
  { path: 'memory/local',   component: MemoryLocalComponent  },
  { path: 'memory/review',  component: MemoryReviewComponent },

  // Scheduler
  { path: 'scheduler/manual', component: SchedulerManualComponent },
  { path: 'scheduler/daily',  component: SchedulerDailyComponent  },
  { path: 'scheduler/retry',  component: SchedulerRetryComponent  },
  { path: 'scheduler/queue',  component: SchedulerQueueComponent  },

  // Settings (Unified)
  { path: 'settings', component: SettingsMainComponent },
  { path: 'settings/:agent', component: SettingsMainComponent },

  // Drive
  { path: 'drive/explorer',        component: DriveExplorerComponent        },
  { path: 'drive/mapping',         component: DriveMappingComponent         },
  { path: 'drive/oauth/callback',  component: DriveOAuthCallbackComponent   },
  { path: 'drive/drive-config',    component: DriveConfigComponent          },

  // Fallback
  { path: '**', redirectTo: 'dashboard/overview' }
];
