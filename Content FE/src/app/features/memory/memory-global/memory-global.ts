import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { MemoryStore } from '../store/memory.store';
import { GlobalMemoryFull } from '../../../core/models/content-factory.models';

@Component({
  selector: 'app-memory-global',
  imports: [CommonModule, FormsModule],
  templateUrl: './memory-global.html',
  styleUrl: './memory-global.css'
})
export class MemoryGlobalComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly memoryStore = inject(MemoryStore);

  activeTab = signal<string>('folders');
  editMode = signal<boolean>(false);
  
  // Create a deep copy for editing
  draftMemory = signal<GlobalMemoryFull | null>(null);

  ngOnInit() {
    this.memoryStore.loadGlobalMemory().then(() => {
      const memory = this.memoryStore.globalMemoryFull();
      if (memory) {
        this.draftMemory.set(JSON.parse(JSON.stringify(memory)));
      }
    });
  }

  setTab(tab: string) {
    this.activeTab.set(tab);
  }

  toggleEdit() {
    this.editMode.set(!this.editMode());
    if (this.editMode()) {
      const memory = this.memoryStore.globalMemoryFull();
      if (memory) {
        this.draftMemory.set(JSON.parse(JSON.stringify(memory)));
      }
    }
  }

  forceRefresh() {
    this.memoryStore.refreshGlobalMemory().then(() => {
      const memory = this.memoryStore.globalMemoryFull();
      if (memory) {
        this.draftMemory.set(JSON.parse(JSON.stringify(memory)));
      }
    });
  }

  saveGlobalMemory() {
    const memory = this.draftMemory();
    if (memory) {
      this.memoryStore.saveGlobalMemory(memory);
      this.editMode.set(false);
    }
  }

  addTierSite(tier: number, site: string) {
    if (!site) return;
    const memory = this.draftMemory();
    if (memory) {
      if (tier === 1) memory.trendConfig.tier1Sites.push(site);
      if (tier === 2) memory.trendConfig.tier2Sites.push(site);
      if (tier === 3) memory.trendConfig.tier3Sites.push(site);
      this.draftMemory.set({...memory});
    }
  }

  removeTierSite(tier: number, index: number) {
    const memory = this.draftMemory();
    if (memory) {
      if (tier === 1) memory.trendConfig.tier1Sites.splice(index, 1);
      if (tier === 2) memory.trendConfig.tier2Sites.splice(index, 1);
      if (tier === 3) memory.trendConfig.tier3Sites.splice(index, 1);
      this.draftMemory.set({...memory});
    }
  }

  updateFolderMapping(agentKey: string, path: string) {
    const memory = this.draftMemory();
    if (memory) {
      memory.folderRegistry[agentKey] = path;
      this.draftMemory.set({...memory});
    }
  }
}
