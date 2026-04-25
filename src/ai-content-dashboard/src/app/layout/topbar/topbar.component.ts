import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../core/store/content-factory.store';

// First side-tab path for each top section
const SECTION_DEFAULT_PATH: Record<string, string> = {
  dashboard: '/dashboard/overview',
  agents:    '/agents/main-brain',
  memory:    '/memory/global',
  scheduler: '/scheduler/manual',
  settings:  '/settings/main-brain',
  drive:     '/drive/explorer'
};

@Component({
  selector: 'app-topbar',
  imports: [CommonModule, RouterLink],
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.css'
})
export class TopbarComponent {
  protected readonly store = inject(ContentFactoryStore);
  private readonly router = inject(Router);

  sectionPath(sectionId: string): string {
    return SECTION_DEFAULT_PATH[sectionId] ?? '/dashboard/overview';
  }
}
