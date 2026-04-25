import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { DashboardPageComponent } from './features/dashboard-page/dashboard-page.component';
import { AgentsPageComponent } from './features/agents-page/agents-page.component';
import { MemoryPageComponent } from './features/memory-page/memory-page.component';
import { SchedulerPageComponent } from './features/scheduler-page/scheduler-page.component';
import { SettingsPageComponent } from './features/settings-page/settings-page.component';
import { SidebarComponent } from './layout/sidebar/sidebar.component';
import { TopbarComponent } from './layout/topbar/topbar.component';
import { ContentFactoryStore } from './core/store/content-factory.store';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    SidebarComponent,
    TopbarComponent,
    DashboardPageComponent,
    AgentsPageComponent,
    MemoryPageComponent,
    SchedulerPageComponent,
    SettingsPageComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly store = inject(ContentFactoryStore);

  ngOnInit(): void {
    void this.store.init();
  }
}
