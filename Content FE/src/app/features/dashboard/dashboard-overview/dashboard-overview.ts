import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { DashboardStore } from '../store/dashboard.store';

@Component({
  selector: 'app-dashboard-overview',
  imports: [CommonModule],
  templateUrl: './dashboard-overview.html',
  styleUrl: './dashboard-overview.css'
})
export class DashboardOverviewComponent {
  protected readonly dashboardStore = inject(DashboardStore);
}
