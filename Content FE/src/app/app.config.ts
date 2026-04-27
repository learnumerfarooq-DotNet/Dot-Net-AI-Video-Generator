import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { ContentFactoryStore } from './core/store/content-factory.store';
import { PipelineStore } from './core/store/pipeline.store';
import { AgentsStore } from './features/agents/store/agents.store';
import { SettingsStore } from './features/settings/store/settings.store';
import { AnalyticsStore } from './core/store/analytics.store';
import { ErrorStore } from './core/store/error.store';
import { DashboardStore } from './features/dashboard/store/dashboard.store';
import { MemoryStore } from './features/memory/store/memory.store';
import { DriveStore } from './features/drive/store/drive.store';
import { SchedulerStore } from './features/scheduler/store/scheduler.store';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    provideRouter(routes, withComponentInputBinding()),
    ContentFactoryStore,
    PipelineStore,
    AgentsStore,
    SettingsStore,
    AnalyticsStore,
    ErrorStore,
    DashboardStore,
    MemoryStore,
    DriveStore,
    SchedulerStore
  ]
};
