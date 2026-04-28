import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../../agents/store/agents.store';
import { SettingsStore } from '../store/settings.store';
import { DriveStore } from '../../drive/store/drive.store';

@Component({
  selector: 'app-settings-main',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './settings-main.html',
  styleUrl: './settings-main.css'
})
export class SettingsMainComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly settingsStore = inject(SettingsStore);
  protected readonly driveStore = inject(DriveStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  
  activeSection: 'agents' | 'global' = 'agents';
  showSecrets: Record<string, boolean> = {};

  providers = ['OpenRouter', 'Gemini', 'OpenAI', 'Claude'];

  ngOnInit() {
    this.settingsStore.loadGlobalSettings();
    this.route.params.subscribe(params => {
      if (params['agent']) {
        this.settingsStore.setActiveAgent(params['agent']);
        this.activeSection = 'agents';
      } else {
        // Default to global or first agent if no param?
        // Let's keep it as is or default to main-brain
      }
    });
  }


  get activeAgent() {
    const key = this.settingsStore.activeAgentKey();
    return this.settingsStore.agentSettings().find(s => s.agentKey === key);
  }

  get activeDraft() {
    const key = this.settingsStore.activeAgentKey();
    return key ? this.settingsStore.settingsDraft(key) : null;
  }

  selectAgent(key: string) {
    this.router.navigate(['/settings', key]);
  }


  toggleSecret(key: string) {
    this.showSecrets[key] = !this.showSecrets[key];
  }

  async saveSettings() {
    const key = this.settingsStore.activeAgentKey();
    if (key) {
      await this.settingsStore.saveAgentSettings(key, () => this.rootStoreRefresh());
    }
  }

  async resetSettings() {
    const key = this.settingsStore.activeAgentKey();
    if (key) {
      await this.settingsStore.resetAgentSettings(key);
    }
  }

  async testConnection() {
    const key = this.settingsStore.activeAgentKey();
    if (key) {
      await this.settingsStore.testAgentConnection(key);
    }
  }

  private async rootStoreRefresh() {
    // Refresh global state if needed
  }
}
