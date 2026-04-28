import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';
import { DriveExplorerComponent } from '../../drive/drive-explorer';

import { SettingsStore } from '../../settings/store/settings.store';

@Component({
  selector: 'app-youtube-agent',
  standalone: true,
  imports: [CommonModule, RouterLink, AgentChatComponent, DriveExplorerComponent],
  templateUrl: './youtube-agent.html',
  styleUrls: ['./youtube-agent.css', '../agent-workspace-shared.css']
})
export class YoutubeAgentComponent implements OnInit {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
  protected readonly settingsStore = inject(SettingsStore);

  ngOnInit(): void {
    // Initialization logic if any
  }

  connectYouTube(): void {
    const agent = this.agentsStore.activeAgent();
    if (!agent) return;

    const redirectUri = window.location.origin + '/agents/youtube-agent';
    this.settingsStore.getYouTubeAuthUrl(agent.key, redirectUri).subscribe({
      next: (url) => {
        window.location.href = url;
      },
      error: (err) => {
        alert('Failed to get YouTube Auth URL. Please check your backend appsettings.json for ClientId/Secret.');
      }
    });
  }
}
