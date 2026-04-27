import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { PipelineStore } from '../../../core/store/pipeline.store';

@Component({
  selector: 'app-processing-videos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './processing-videos.html',
  styleUrl: './processing-videos.css'
})
export class ProcessingVideosComponent {
  protected readonly pipelineStore = inject(PipelineStore);

  get activeJobs() {
    return this.pipelineStore.jobs().filter(j => j.status !== 'AnalyticsCollected' && j.status !== 'Published' && j.status !== 'Failed');
  }
}
