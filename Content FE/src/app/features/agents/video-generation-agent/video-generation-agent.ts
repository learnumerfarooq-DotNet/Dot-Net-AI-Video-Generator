import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-video-generation-agent',
  imports: [CommonModule, ReactiveFormsModule, AgentChatComponent],
  templateUrl: './video-generation-agent.html',
  styleUrl: './video-generation-agent.css'
})
export class VideoGenerationAgentComponent {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
