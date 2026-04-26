import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';

@Component({
  selector: 'app-main-brain',
  imports: [CommonModule, ReactiveFormsModule, AgentChatComponent],
  templateUrl: './main-brain.html',
  styleUrl: './main-brain.css'
})
export class MainBrainComponent {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
