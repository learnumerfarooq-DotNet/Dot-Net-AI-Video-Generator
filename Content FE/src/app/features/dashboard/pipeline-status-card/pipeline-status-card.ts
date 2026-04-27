import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardStore } from '../store/dashboard.store';

@Component({
  selector: 'app-pipeline-status-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pipeline-status-card.html',
  styleUrl: './pipeline-status-card.css'
})
export class PipelineStatusCardComponent {
  private store = inject(DashboardStore);

  // Mock computed signals for now until store is fully wired with real pipeline data
  activeJobsCount = computed(() => this.store.readyCount() + 2); // Mock
  completedToday = computed(() => this.store.publishedCount() + 5); // Mock
  failedToday = computed(() => 0); // Mock

  stages = ['RAW', 'Script', 'Edit', 'Shorts', 'Short Edit', 'Upload', 'Published'];

  activeJobs = computed(() => [
    { name: 'AI Tutorial Video', stage: 'Edit', progress: 60, startedAt: new Date(Date.now() - 3600000), duration: '1h 5m' },
    { name: 'Top 5 Tech Trends', stage: 'Script', progress: 30, startedAt: new Date(Date.now() - 1800000), duration: '30m' }
  ]);

  getStageClass(stage: string): Record<string, boolean> {
    const isActive = this.activeJobs().some(j => j.stage === stage);
    return {
      'bg-primary/20 border-primary text-primary shadow-[0_0_15px_rgba(59,130,246,0.3)]': isActive,
      'bg-bg-deep border-border-border text-text-muted': !isActive
    };
  }

  getStageIcon(stage: string): string {
    switch (stage) {
      case 'RAW': return 'fa-solid fa-file-video';
      case 'Script': return 'fa-solid fa-pen-nib';
      case 'Edit': return 'fa-solid fa-film';
      case 'Shorts': return 'fa-solid fa-scissors';
      case 'Short Edit': return 'fa-solid fa-wand-magic-sparkles';
      case 'Upload': return 'fa-solid fa-cloud-arrow-up';
      case 'Published': return 'fa-solid fa-check-double';
      default: return 'fa-solid fa-circle';
    }
  }
}
