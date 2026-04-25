import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../core/store/content-factory.store';
import { MemorySuggestionDto } from '../../core/models/content-factory.models';
import { computed } from '@angular/core';

@Component({
  selector: 'app-memory-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './memory-page.component.html',
  styleUrl: './memory-page.component.css'
})
export class MemoryPageComponent {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly pendingMemorySuggestions = computed<MemorySuggestionDto[]>(() => this.store.pendingMemorySuggestions());
}
