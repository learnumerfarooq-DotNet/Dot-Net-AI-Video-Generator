import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-instagram-agent',
  imports: [CommonModule, RouterLink, AgentChatComponent],
  templateUrl: './instagram-agent.html',
  styleUrl: './instagram-agent.css'
})
export class InstagramAgentComponent {
  protected readonly store = inject(ContentFactoryStore);
}
