import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-shorts-agent-1',
  imports: [CommonModule, ReactiveFormsModule, AgentChatComponent],
  templateUrl: './shorts-agent-1.html',
  styleUrl: './shorts-agent-1.css'
})
export class ShortsAgent1Component {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
