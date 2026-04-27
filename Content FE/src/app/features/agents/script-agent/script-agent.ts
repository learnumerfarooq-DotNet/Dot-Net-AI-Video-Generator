import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';
import { DriveExplorerComponent } from '../../drive/drive-explorer';

@Component({
  selector: 'app-script-agent',
  imports: [CommonModule, ReactiveFormsModule, AgentChatComponent, DriveExplorerComponent],
  templateUrl: './script-agent.html',
  styleUrls: ['./script-agent.css', '../agent-workspace-shared.css']
})
export class ScriptAgentComponent {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
