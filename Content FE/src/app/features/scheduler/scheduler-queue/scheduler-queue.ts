import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { SchedulerStore } from '../store/scheduler.store';

@Component({
  selector: 'app-scheduler-queue',
  imports: [CommonModule],
  templateUrl: './scheduler-queue.html',
  styleUrl: './scheduler-queue.css'
})
export class SchedulerQueueComponent {
  protected readonly schedulerStore = inject(SchedulerStore);
}
