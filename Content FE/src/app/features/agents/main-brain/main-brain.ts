import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-main-brain',
  imports: [CommonModule, RouterLink, ReactiveFormsModule, AgentChatComponent],
  templateUrl: './main-brain.html',
  styleUrls: ['./main-brain.css', '../agent-workspace-shared.css']
})
export class MainBrainComponent {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);

  // Agent State
  status = signal<'idle' | 'running' | 'error'>('idle');
  modelName = signal('llama-3.1-8b-instruct:free');
  
  // Mock Data for UI
  brainState = signal({
    tick: 12456,
    activeJobs: 3,
    circuitBreaker: 'Closed'
  });

  localMemory = signal({
    'Role': 'Orchestrator',
    'Goal': 'Produce content autonomously',
    'Tone': 'Professional'
  });

  runHistory = signal([
    { status: 'Completed', startedAt: '10 mins ago', summary: 'Orchestrated Trend Analysis' },
    { status: 'Completed', startedAt: '1 hour ago', summary: 'Delegated Video Edit' }
  ]);

  errorLog = signal([
    { message: 'Timeout waiting for Trend Agent', timestamp: '5 hours ago' }
  ]);

  toggleBrainPause() {
    console.log('Toggling Brain Pause');
  }

  startRun() {
    this.status.set('running');
    const agent = this.agentsStore.activeAgent();
    if(agent) {
      this.agentsStore.startAgentRun(agent.key);
    }
  }

  stopRun() {
    this.status.set('idle');
    const agent = this.agentsStore.activeAgent();
    if(agent) {
      this.agentsStore.stopAgentRun(agent.key);
    }
  }

  viewMemory(agentKey: string) {
    this.rootStore.setSection('memory');
    this.rootStore.setSideTab('memory-local');
  }
}
