import { CommonModule } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../core/store/content-factory.store';

// Map every SideTabId to its URL path
const TAB_TO_PATH: Record<string, string> = {
  // Dashboard
  'dashboard-overview':   '/dashboard/overview',
  'dashboard-usage':      '/dashboard/usage',
  'dashboard-memory':     '/dashboard/memory',
  'dashboard-drive':      '/dashboard/drive',
  'dashboard-published':  '/dashboard/published',
  'dashboard-growth':     '/dashboard/growth',
  // Agents
  'main-brain':              '/agents/main-brain',
  'trend-agent':             '/agents/trend-agent',
  'script-agent':            '/agents/script-agent',
  'video-generation-agent':  '/agents/video-generation-agent',
  'shorts-agent-1':          '/agents/shorts-agent-1',
  'shorts-agent-2':          '/agents/shorts-agent-2',
  'youtube-agent':           '/agents/youtube-agent',
  'tiktok-agent':            '/agents/tiktok-agent',
  'instagram-agent':         '/agents/instagram-agent',
  'facebook-agent':          '/agents/facebook-agent',
  'linkedin-agent':          '/agents/linkedin-agent',
  // Memory
  'memory-global':  '/memory/global',
  'memory-local':   '/memory/local',
  'memory-review':  '/memory/review',
  // Scheduler
  'scheduler-manual': '/scheduler/manual',
  'scheduler-daily':  '/scheduler/daily',
  'scheduler-retry':  '/scheduler/retry',
  'scheduler-queue':  '/scheduler/queue',
  // Settings
  'settings-main-brain':              '/settings/main-brain',
  'settings-trend-agent':             '/settings/trend-agent',
  'settings-script-agent':            '/settings/script-agent',
  'settings-video-generation-agent':  '/settings/video-generation-agent',
  'settings-shorts-agent-1':          '/settings/shorts-agent-1',
  'settings-shorts-agent-2':          '/settings/shorts-agent-2',
  'settings-youtube-agent':           '/settings/youtube-agent',
  'settings-tiktok-agent':            '/settings/tiktok-agent',
  'settings-instagram-agent':         '/settings/instagram-agent',
  'settings-facebook-agent':          '/settings/facebook-agent',
  'settings-linkedin-agent':          '/settings/linkedin-agent',
  // Drive
  'drive-explorer': '/drive/explorer',
  'drive-config':   '/drive/config'
};

@Component({
  selector: 'app-sidebar',
  imports: [CommonModule, RouterLink],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
  host: {
    '[class.collapsed]': 'collapsed()'
  }
})
export class SidebarComponent {
  protected readonly store = inject(ContentFactoryStore);
  readonly collapsed = input(false);

  tabPath(tabId: string): string {
    return TAB_TO_PATH[tabId] ?? '/dashboard/overview';
  }
}
