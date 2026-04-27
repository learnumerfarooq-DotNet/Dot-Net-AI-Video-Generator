import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { SchedulerStore } from '../store/scheduler.store';

@Component({
  selector: 'app-scheduler-queue',
  imports: [CommonModule],
  templateUrl: './scheduler-queue.html',
  styleUrl: './scheduler-queue.css'
})
export class SchedulerQueueComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly schedulerStore = inject(SchedulerStore);

  ngOnInit() {
    this.schedulerStore.getQueueStats();
  }
}
