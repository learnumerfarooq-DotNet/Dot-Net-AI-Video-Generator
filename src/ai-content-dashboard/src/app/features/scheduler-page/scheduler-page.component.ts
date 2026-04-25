import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../core/store/content-factory.store';

@Component({
  selector: 'app-scheduler-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './scheduler-page.component.html',
  styleUrl: './scheduler-page.component.css'
})
export class SchedulerPageComponent {
  protected readonly store = inject(ContentFactoryStore);
}
