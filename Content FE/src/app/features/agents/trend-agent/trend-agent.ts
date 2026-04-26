import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-trend-agent',
  imports: [CommonModule, ReactiveFormsModule, AgentChatComponent],
  templateUrl: './trend-agent.html',
  styleUrl: './trend-agent.css'
})
export class TrendAgentComponent {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
