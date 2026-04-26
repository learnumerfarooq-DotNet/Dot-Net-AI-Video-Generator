import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { DashboardStore } from '../store/dashboard.store';

@Component({
  selector: 'app-dashboard-drive',
  imports: [CommonModule],
  templateUrl: './dashboard-drive.html',
  styleUrl: './dashboard-drive.css'
})
export class DashboardDriveComponent {
  protected readonly dashboardStore = inject(DashboardStore);
}
