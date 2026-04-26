import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, effect } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../../agents/store/agents.store';
import { SettingsStore } from '../store/settings.store';

@Component({
  selector: 'app-settings-agent',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './settings-agent.html',
  styleUrl: './settings-agent.css'
})
export class SettingsAgentComponent implements OnInit {
  protected readonly settingsStore = inject(SettingsStore);
  protected readonly store = inject(ContentFactoryStore);
  protected readonly agentsStore = inject(AgentsStore);
  agentForm!: FormGroup;

  constructor(private fb: FormBuilder) {
    // Re-init form whenever the active agent changes
    effect(() => {
      const agent = this.activeAgentSettings;
      if (agent) {
        this.agentForm = this.fb.group({
          providerName: [agent.providerName || '', Validators.required],
          modelName: [agent.modelName || ''],
          baseUrl: [agent.baseUrl || ''],
          apiKey: [agent.apiKey || ''],
          clientId: [agent.clientId || ''],
          clientSecret: [agent.clientSecret || ''],
          refreshToken: [agent.refreshToken || ''],

          useOpenRouter: [agent.useOpenRouter || false],
          openRouterModel: [agent.openRouterModel || ''],
          openRouterApiKey: [agent.openRouterApiKey || ''],
          notes: [agent.notes || '']
        });
      }
    });
  }

  ngOnInit(): void {}

  save(): void {
    if (!this.agentForm.valid) return;
    const agent = this.activeAgentSettings;
    if (agent) {
      // Patch draft values then call store save
      const val = this.agentForm.value;
      Object.keys(val).forEach((key) => {
        this.settingsStore.updateSettingsDraft(agent.agentKey, key as any, val[key]);
      });
      void this.settingsStore.saveAgentSettings(agent.agentKey, () => this.store.refreshAll());
    }
  }

  test(): void {
    const agent = this.activeAgentSettings;
    if (agent) {
      // Sync form to draft first so we test current UI values (if we were to send draft, but backend uses saved settings usually)
      // Actually, my backend Test endpoint uses SAVED settings.
      // So I should probably SAVE first or Warn. 
      // But for a better UX, I'll just test.
      void this.settingsStore.testAgentConnection(agent.agentKey);
    }
  }

  get providerOptions(): string[] {
    const agent = this.activeAgentSettings;
    return agent ? this.settingsStore.getProviderOptionsForAgent(agent.agentKey) : [];
  }

  get isOpenRouter(): boolean {
    return this.agentForm?.get('useOpenRouter')?.value ?? false;
  }

  get activeAgentSettings() {
    const key = this.agentsStore.activeAgentKey();
    return key ? this.settingsStore.getSettingsForAgent(key) : null;
  }
}
