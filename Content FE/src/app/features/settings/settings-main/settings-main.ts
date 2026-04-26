import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SettingsAgentComponent } from '../settings-agent/settings-agent';
import { DriveConfigComponent } from '../../drive/drive-config';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../../agents/store/agents.store';
import { SettingsStore } from '../store/settings.store';

@Component({
  selector: 'app-settings-main',
  standalone: true,
  imports: [CommonModule, SettingsAgentComponent, DriveConfigComponent],
  templateUrl: './settings-main.html',
  styleUrl: './settings-main.css'
})
export class SettingsMainComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly settingsStore = inject(SettingsStore);
  private readonly route = inject(ActivatedRoute);
  
  activeTab: 'agents' | 'storage' = 'agents';

  ngOnInit() {
    const agentParam = this.route.snapshot.paramMap.get('agent');
    if (agentParam) {
      this.store.setActiveAgent(agentParam);
    }
  }

  setTab(tab: 'agents' | 'storage') {
    this.activeTab = tab;
  }

  selectAgent(key: string) {
    this.store.setActiveAgent(key);
  }

  get agents() {
    return this.store.agents();
  }

  get activeAgentKey() {
    return this.agentsStore.activeAgentKey();
  }
}
