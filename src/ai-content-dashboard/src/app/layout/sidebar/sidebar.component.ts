import { CommonModule } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { ContentFactoryStore } from '../../core/store/content-factory.store';

@Component({
  selector: 'app-sidebar',
  imports: [CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
  host: {
    '[class.collapsed]': 'collapsed()'
  }
})
export class SidebarComponent {
  protected readonly store = inject(ContentFactoryStore);
  readonly collapsed = input(false);
}
