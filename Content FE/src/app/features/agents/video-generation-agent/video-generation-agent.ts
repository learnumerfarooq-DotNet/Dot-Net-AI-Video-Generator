import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';
import { DualDriveExplorerComponent } from '../../../shared/components/dual-drive-explorer/dual-drive-explorer';
import { ProcessingVideosComponent } from '../../../shared/components/processing-videos/processing-videos';

@Component({
  selector: 'app-video-generation-agent',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AgentChatComponent, DualDriveExplorerComponent, ProcessingVideosComponent],
  templateUrl: './video-generation-agent.html',
  styleUrls: ['./video-generation-agent.css', '../agent-workspace-shared.css']
})
export class VideoGenerationAgentComponent {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
}
