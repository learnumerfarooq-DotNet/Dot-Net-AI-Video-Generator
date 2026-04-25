import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';

@Component({
  selector: 'app-memory-local',
  imports: [CommonModule],
  templateUrl: './memory-local.html',
  styleUrl: './memory-local.css'
})
export class MemoryLocalComponent {
  protected readonly store = inject(ContentFactoryStore);
}
