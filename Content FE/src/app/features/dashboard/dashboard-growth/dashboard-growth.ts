import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';

@Component({
  selector: 'app-dashboard-growth',
  imports: [CommonModule],
  templateUrl: './dashboard-growth.html',
  styleUrl: './dashboard-growth.css'
})
export class DashboardGrowthComponent {
  protected readonly store = inject(ContentFactoryStore);
}
