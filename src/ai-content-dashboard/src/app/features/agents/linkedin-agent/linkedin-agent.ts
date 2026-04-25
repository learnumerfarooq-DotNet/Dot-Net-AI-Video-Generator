import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-linkedin-agent',
  imports: [CommonModule, RouterLink, AgentChatComponent],
  templateUrl: './linkedin-agent.html',
  styleUrl: './linkedin-agent.css'
})
export class LinkedinAgentComponent {
  protected readonly store = inject(ContentFactoryStore);
}
