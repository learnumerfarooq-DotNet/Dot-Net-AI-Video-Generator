import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';
import { DriveExplorerComponent } from '../../drive/drive-explorer';

@Component({
  selector: 'app-trend-agent',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AgentChatComponent, DriveExplorerComponent],
  templateUrl: './trend-agent.html',
  styleUrls: ['./trend-agent.css', '../agent-workspace-shared.css']
})
export class TrendAgentComponent {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
