import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { SchedulerStore } from '../store/scheduler.store';

@Component({
  selector: 'app-scheduler-retry',
  imports: [CommonModule],
  templateUrl: './scheduler-retry.html',
  styleUrl: './scheduler-retry.css'
})
export class SchedulerRetryComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly schedulerStore = inject(SchedulerStore);
  
  showDeadLetters = false;

  ngOnInit() {
    this.schedulerStore.getRetryQueue();
    this.schedulerStore.getDeadLetterQueue();
  }

  retryNow(jobId: string) {
    this.schedulerStore.retryNow(jobId);
  }

  moveToDeadLetter(jobId: string) {
    if (confirm('Are you sure you want to move this to the Dead Letter queue?')) {
      this.schedulerStore.moveToDeadLetter(jobId);
    }
  }

  resolveDeadLetter(jobId: string) {
    const resolution = prompt('Enter resolution notes for this dead letter:');
    if (resolution !== null) {
      this.schedulerStore.resolveDeadLetter(jobId, resolution);
    }
  }

  toggleDeadLetters() {
    this.showDeadLetters = !this.showDeadLetters;
  }
}
