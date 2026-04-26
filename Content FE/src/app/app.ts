import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { ContentFactoryStore } from './core/store/content-factory.store';
import { SidebarComponent } from './layout/sidebar/sidebar.component';
import { TopbarComponent } from './layout/topbar/topbar.component';

// Mapping from URL path segment → store SideTabId / TopSectionId
const ROUTE_TO_SECTION: Record<string, string> = {
  'dashboard': 'dashboard',
  'agents':    'agents',
  'memory':    'memory',
  'scheduler': 'scheduler',
  'settings':  'settings',
  'drive':     'drive'
};

const ROUTE_TO_TAB: Record<string, string> = {
  // Dashboard
  'dashboard/overview':   'dashboard-overview',
  'dashboard/usage':      'dashboard-usage',
  'dashboard/memory':     'dashboard-memory',
  'dashboard/drive':      'dashboard-drive',
  'dashboard/published':  'dashboard-published',
  'dashboard/growth':     'dashboard-growth',
  // Agents
  'agents/main-brain':              'main-brain',
  'agents/trend-agent':             'trend-agent',
  'agents/script-agent':            'script-agent',
  'agents/video-generation-agent':  'video-generation-agent',
  'agents/shorts-agent-1':          'shorts-agent-1',
  'agents/shorts-agent-2':          'shorts-agent-2',
  'agents/youtube-agent':           'youtube-agent',
  'agents/tiktok-agent':            'tiktok-agent',
  'agents/instagram-agent':         'instagram-agent',
  'agents/facebook-agent':          'facebook-agent',
  'agents/linkedin-agent':          'linkedin-agent',
  // Memory
  'memory/global':  'memory-global',
  'memory/local':   'memory-local',
  'memory/review':  'memory-review',
  // Scheduler
  'scheduler/manual': 'scheduler-manual',
  'scheduler/daily':  'scheduler-daily',
  'scheduler/retry':  'scheduler-retry',
  'scheduler/queue':  'scheduler-queue',
  // Settings
  'settings/main-brain':              'settings-main-brain',
  'settings/trend-agent':             'settings-trend-agent',
  'settings/script-agent':            'settings-script-agent',
  'settings/video-generation-agent':  'settings-video-generation-agent',
  'settings/shorts-agent-1':          'settings-shorts-agent-1',
  'settings/shorts-agent-2':          'settings-shorts-agent-2',
  'settings/youtube-agent':           'settings-youtube-agent',
  'settings/tiktok-agent':            'settings-tiktok-agent',
  'settings/instagram-agent':         'settings-instagram-agent',
  'settings/facebook-agent':          'settings-facebook-agent',
  'settings/linkedin-agent':          'settings-linkedin-agent',
  // Drive
  'drive/explorer': 'drive-explorer',
  'drive/config':   'drive-config'
};

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, SidebarComponent, TopbarComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  private readonly router = inject(Router);

  ngOnInit(): void {
    void this.store.init();

    // Keep store in sync whenever the router navigates
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((event) => {
        const path = event.urlAfterRedirects.replace(/^\//, '').split('?')[0];
        const section = ROUTE_TO_SECTION[path.split('/')[0]];
        const tab = ROUTE_TO_TAB[path];
        if (section) this.store.setSection(section as any);
        if (tab)     this.store.setSideTab(tab as any);
      });
  }
}
