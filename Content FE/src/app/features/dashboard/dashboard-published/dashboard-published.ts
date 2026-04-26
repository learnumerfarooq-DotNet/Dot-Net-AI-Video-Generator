import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { DashboardStore } from '../store/dashboard.store';

@Component({
  selector: 'app-dashboard-published',
  imports: [CommonModule],
  templateUrl: './dashboard-published.html',
  styleUrl: './dashboard-published.css'
})
export class DashboardPublishedComponent {
  protected readonly dashboardStore = inject(DashboardStore);
}
