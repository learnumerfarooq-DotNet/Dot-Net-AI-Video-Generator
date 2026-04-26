import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { SchedulerStore } from '../store/scheduler.store';

@Component({
  selector: 'app-scheduler-retry',
  imports: [CommonModule],
  templateUrl: './scheduler-retry.html',
  styleUrl: './scheduler-retry.css'
})
export class SchedulerRetryComponent {
  protected readonly schedulerStore = inject(SchedulerStore);
}
