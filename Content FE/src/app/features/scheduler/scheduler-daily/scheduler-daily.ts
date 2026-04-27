import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { SchedulerStore } from '../store/scheduler.store';

@Component({
  selector: 'app-scheduler-daily',
  imports: [CommonModule],
  templateUrl: './scheduler-daily.html',
  styleUrl: './scheduler-daily.css'
})
export class SchedulerDailyComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly schedulerStore = inject(SchedulerStore);

  hours = Array.from({length: 24}, (_, i) => i);
  
  ngOnInit() {
    // Ideally call API to refresh
  }

  getJobsForHour(hour: number) {
    // Mock implementation for daily schedule grouping
    return this.schedulerStore.dailyPostingJobs().filter(j => {
      if (!j.nextRunAt) return false;
      const date = new Date(j.nextRunAt);
      return date.getHours() === hour;
    });
  }

  formatHour(hour: number) {
    if (hour === 0) return '12:00 AM';
    if (hour < 12) return `${hour}:00 AM`;
    if (hour === 12) return '12:00 PM';
    return `${hour - 12}:00 PM`;
  }
}
