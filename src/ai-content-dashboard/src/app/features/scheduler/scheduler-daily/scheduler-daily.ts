import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';

@Component({
  selector: 'app-scheduler-daily',
  imports: [CommonModule],
  templateUrl: './scheduler-daily.html',
  styleUrl: './scheduler-daily.css'
})
export class SchedulerDailyComponent {
  protected readonly store = inject(ContentFactoryStore);
}
