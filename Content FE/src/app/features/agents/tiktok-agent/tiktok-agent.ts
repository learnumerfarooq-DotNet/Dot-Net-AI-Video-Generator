import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-tiktok-agent',
  imports: [CommonModule, RouterLink, AgentChatComponent],
  templateUrl: './tiktok-agent.html',
  styleUrl: './tiktok-agent.css'
})
export class TiktokAgentComponent {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
