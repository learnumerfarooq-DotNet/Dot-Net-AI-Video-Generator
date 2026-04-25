import { CommonModule } from '@angular/common';
import { Component, Input, inject, AfterViewChecked, ElementRef, ViewChild, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../core/store/content-factory.store';
import { AgentSummary } from '../../core/models/content-factory.models';

@Component({
  selector: 'app-agent-chat',
  imports: [CommonModule, FormsModule],
  templateUrl: './agent-chat.html',
  styleUrl: './agent-chat.css'
})
export class AgentChatComponent implements AfterViewChecked {
  @Input() agent!: AgentSummary;
  @ViewChild('chatHistoryRef') private chatHistoryRef!: ElementRef<HTMLDivElement>;

  protected readonly store = inject(ContentFactoryStore);

  private shouldScroll = true;

  constructor() {
    // When messages change, mark that we need to scroll
    effect(() => {
      this.store.activeAgentMessages(); // track signal
      this.shouldScroll = true;
    });
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  private scrollToBottom(): void {
    try {
      const el = this.chatHistoryRef?.nativeElement;
      if (el) {
        el.scrollTop = el.scrollHeight;
      }
    } catch { /* ignore */ }
  }

  protected onKeyDown(event: Event): void {
    const key = event as KeyboardEvent;
    if (!key.shiftKey) {
      key.preventDefault();
      if (this.store.chatDraft()) {
        void this.store.sendAgentMessage();
      }
    }
  }
}
