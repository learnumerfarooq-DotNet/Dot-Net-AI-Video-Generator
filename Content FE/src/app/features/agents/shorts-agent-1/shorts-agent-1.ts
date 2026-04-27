import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';
import { DualDriveExplorerComponent } from '../../../shared/components/dual-drive-explorer/dual-drive-explorer';
import { ProcessingVideosComponent } from '../../../shared/components/processing-videos/processing-videos';

@Component({
  selector: 'app-shorts-agent-1',
  imports: [CommonModule, ReactiveFormsModule, AgentChatComponent, DualDriveExplorerComponent, ProcessingVideosComponent],
  templateUrl: './shorts-agent-1.html',
  styleUrls: ['./shorts-agent-1.css', '../agent-workspace-shared.css']
})
export class ShortsAgent1Component {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
