import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';

@Component({
  selector: 'app-memory-global',
  imports: [CommonModule],
  templateUrl: './memory-global.html',
  styleUrl: './memory-global.css'
})
export class MemoryGlobalComponent {
  protected readonly store = inject(ContentFactoryStore);
}
