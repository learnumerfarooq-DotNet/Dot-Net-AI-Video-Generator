import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { MemoryStore } from '../../memory/store/memory.store';

@Component({
  selector: 'app-dashboard-memory',
  imports: [CommonModule],
  templateUrl: './dashboard-memory.html',
  styleUrl: './dashboard-memory.css'
})
export class DashboardMemoryComponent {
  protected readonly memoryStore = inject(MemoryStore);
}
