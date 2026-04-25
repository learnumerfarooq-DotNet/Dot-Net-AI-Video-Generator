import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-facebook-agent',
  imports: [CommonModule, RouterLink, AgentChatComponent],
  templateUrl: './facebook-agent.html',
  styleUrl: './facebook-agent.css'
})
export class FacebookAgentComponent {
  protected readonly store = inject(ContentFactoryStore);
}
