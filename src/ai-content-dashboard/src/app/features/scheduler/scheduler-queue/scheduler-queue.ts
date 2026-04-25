import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';

@Component({
  selector: 'app-scheduler-queue',
  imports: [CommonModule],
  templateUrl: './scheduler-queue.html',
  styleUrl: './scheduler-queue.css'
})
export class SchedulerQueueComponent {
  protected readonly store = inject(ContentFactoryStore);
}
