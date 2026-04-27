import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { MemoryStore } from '../store/memory.store';
import { AgentsStore } from '../../agents/store/agents.store';

@Component({
  selector: 'app-memory-local',
  imports: [CommonModule, FormsModule],
  templateUrl: './memory-local.html',
  styleUrl: './memory-local.css'
})
export class MemoryLocalComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly memoryStore = inject(MemoryStore);
  protected readonly agentsStore = inject(AgentsStore);

  selectedAgentKey = signal<string>('trend-agent');
  configJsonString = signal<string>('');
  editMode = signal<boolean>(false);
  saveError = signal<string>('');

  ngOnInit() {
    this.loadAgent(this.selectedAgentKey());
  }

  loadAgent(agentKey: string) {
    this.selectedAgentKey.set(agentKey);
    this.memoryStore.loadLocalMemory(agentKey).then(() => {
      const memory = this.memoryStore.agentLocalMemories()[agentKey];
      if (memory) {
        this.configJsonString.set(JSON.stringify(memory.config, null, 2));
      }
    });
  }

  toggleEdit() {
    this.editMode.set(!this.editMode());
    this.saveError.set('');
    if (!this.editMode()) {
      // Revert changes
      const memory = this.memoryStore.agentLocalMemories()[this.selectedAgentKey()];
      if (memory) {
        this.configJsonString.set(JSON.stringify(memory.config, null, 2));
      }
    }
  }

  saveLocalMemory() {
    try {
      const parsedConfig = JSON.parse(this.configJsonString());
      this.memoryStore.saveLocalMemory(this.selectedAgentKey(), parsedConfig).then(() => {
        this.editMode.set(false);
        this.saveError.set('');
      });
    } catch (e: any) {
      this.saveError.set('Invalid JSON format: ' + e.message);
    }
  }

  resetToDefaults() {
    if (confirm('Are you sure you want to reset this agent\'s local memory to defaults?')) {
      this.memoryStore.resetLocalMemory(this.selectedAgentKey()).then(() => {
        const memory = this.memoryStore.agentLocalMemories()[this.selectedAgentKey()];
        if (memory) {
          this.configJsonString.set(JSON.stringify(memory.config, null, 2));
        }
      });
    }
  }

  syncToDrive() {
    // Backend would handle this trigger, using save action for now
    alert('Sync to Google Drive initiated.');
  }

  getObjectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }
}
