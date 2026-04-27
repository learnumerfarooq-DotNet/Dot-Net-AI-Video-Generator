import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardStore } from '../store/dashboard.store';

@Component({
  selector: 'app-brain-status-widget',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './brain-status-widget.html',
  styleUrl: './brain-status-widget.css'
})
export class BrainStatusWidgetComponent {
  private store = inject(DashboardStore);

  // Mocking data for now until fully wired
  brainStatus = computed(() => 'Watching');
  currentTick = computed(() => 12456);
  lastTickAgo = computed(() => '30 seconds ago');
  activeJobs = computed(() => this.store.readyCount() + 2);
  circuitBreakerOpen = computed(() => false);
  globalMemoryVersion = computed(() => 'v1.2 (synced 1 min ago)');
  isPaused = computed(() => false);

  togglePause() {
    console.log('Toggling brain pause state');
  }
}
