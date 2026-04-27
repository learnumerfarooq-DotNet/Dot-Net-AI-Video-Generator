import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';
import { DriveExplorerComponent } from '../../drive/drive-explorer';

@Component({
  selector: 'app-instagram-agent',
  imports: [CommonModule, RouterLink, AgentChatComponent, DriveExplorerComponent],
  templateUrl: './instagram-agent.html',
  styleUrls: ['./instagram-agent.css', '../agent-workspace-shared.css']
})
export class InstagramAgentComponent {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
