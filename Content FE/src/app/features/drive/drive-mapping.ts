import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AgentsStore } from '../agents/store/agents.store';
import { SettingsStore } from '../settings/store/settings.store';
import { DriveStore } from './store/drive.store';
import { ContentFactoryStore } from '../../core/store/content-factory.store';
import { MemoryStore } from '../memory/store/memory.store';

@Component({
  selector: 'app-drive-mapping',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './drive-mapping.html',
  styleUrl: './drive-mapping.css'
})
export class DriveMappingComponent implements OnInit {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly settingsStore = inject(SettingsStore);
  protected readonly driveStore = inject(DriveStore);
  protected readonly rootStore = inject(ContentFactoryStore);
  protected readonly memoryStore = inject(MemoryStore);

  showFolderPicker = false;
  selectedAgentKey: string | null = null;
  showMappingTable = true;

  ngOnInit() {
    this.driveStore.loadFolderMappings();
  }

  getFolderRegistry() {
    return this.memoryStore.globalMemoryFull()?.folderRegistry || {};
  }

  get registryKeys() {
    return Object.keys(this.getFolderRegistry());
  }

  async createAllMissing() {
    await this.driveStore.createMissingFolders();
  }

  getMappingStatus(agentKey: string): { icon: string, color: string, text: string } {
    // This would ideally come from driveStore.folderMappings
    const mapping = this.driveStore.folderMappings()?.[agentKey];
    if (!mapping) return { icon: 'fa-circle-question', color: 'text-text-secondary', text: 'Unknown' };
    
    if (mapping.exists) {
       return { icon: 'fa-circle-check', color: 'text-accent-green', text: 'Exists' };
    }
    return { icon: 'fa-circle-xmark', color: 'text-accent-red', text: 'Missing' };
  }

  browseFolder(agentKey: string) {
    this.selectedAgentKey = agentKey;
    this.showFolderPicker = true;
    this.driveStore.loadDriveFiles();
  }

  async selectFolder(folder: any) {
    if (this.selectedAgentKey) {
      // In a real app, this would update the folder registry in Global Memory
      console.log(`Mapping ${this.selectedAgentKey} to ${folder.name} (${folder.id})`);
      this.showFolderPicker = false;
      this.selectedAgentKey = null;
    }
  }
}
