import { CommonModule } from '@angular/common';
import { Component, Input, inject, AfterViewChecked, ElementRef, ViewChild, effect, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AgentsStore } from '../../features/agents/store/agents.store';
import { AgentSummary } from '../../core/models/content-factory.models';
import { ContentFactoryStore } from '../../core/store/content-factory.store';

@Component({
  selector: 'app-agent-chat',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './agent-chat.html',
  styleUrl: './agent-chat.css'
})
export class AgentChatComponent implements OnInit, AfterViewChecked {
  @Input() agent!: AgentSummary;
  @ViewChild('chatHistoryRef') private chatHistoryRef!: ElementRef<HTMLDivElement>;

  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);

  private shouldScroll = true;

  constructor() {
    // When messages change, mark that we need to scroll
    effect(() => {
      this.agentsStore.activeAgentMessages(); // track signal
      this.shouldScroll = true;
    });
  }

  ngOnInit(): void {
    if (this.agent?.key) {
      this.agentsStore.setActiveAgentKey(this.agent.key);
    }
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
    if (key.key === 'Enter' && !key.shiftKey) {
      key.preventDefault();
      if (this.agentsStore.chatDraft()) {
        this.sendMessage();
      }
    }
  }

  sendMessage(): void {
    void this.agentsStore.sendAgentMessage(() => this.rootStore.refreshAll());
  }
}
