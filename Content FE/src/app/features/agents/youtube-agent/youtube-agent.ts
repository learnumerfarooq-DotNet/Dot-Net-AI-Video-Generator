import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';
import { DriveExplorerComponent } from '../../drive/drive-explorer';

@Component({
  selector: 'app-youtube-agent',
  imports: [CommonModule, RouterLink, AgentChatComponent, DriveExplorerComponent],
  templateUrl: './youtube-agent.html',
  styleUrls: ['./youtube-agent.css', '../agent-workspace-shared.css']
})
export class YoutubeAgentComponent implements OnInit {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);

  ngOnInit(): void {
    // Initialization logic if any
  }
}
