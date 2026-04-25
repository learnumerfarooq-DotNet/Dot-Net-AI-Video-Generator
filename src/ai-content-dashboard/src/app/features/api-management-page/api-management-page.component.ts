import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

type ProviderField = 'textProvider' | 'videoProvider' | 'uploadProvider' | 'storageProvider';

type ProviderSelections = Record<ProviderField, string>;

type RequirementField = {
  key: string;
  label: string;
  inputType: 'text' | 'password';
  helpText: string;
};

type ProviderRequirement = {
  id: string;
  providerType: string;
  displayName: string;
  documentationUrl?: string;
  notes: string;
  fields: RequirementField[];
};

@Component({
  selector: 'app-api-management-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './api-management-page.component.html',
  styleUrl: './api-management-page.component.css'
})
export class ApiManagementPageComponent {
  protected readonly providerOptions: Record<ProviderField, string[]> = {
    textProvider: ['OpenAI', 'OpenRouter', 'Anthropic', 'Google AI Studio'],
    videoProvider: ['Runway', 'Pika', 'Synthesia', 'HeyGen'],
    uploadProvider: ['YouTube Data API', 'TikTok API', 'Meta Graph API', 'LinkedIn API'],
    storageProvider: ['Google Drive', 'Dropbox', 'OneDrive', 'Local Filesystem']
  };

  protected readonly providers: ProviderSelections = {
    textProvider: 'OpenAI',
    videoProvider: 'Runway',
    uploadProvider: 'YouTube Data API',
    storageProvider: 'Google Drive'
  };

  protected readonly requirementCatalog: Record<string, ProviderRequirement> = {
    OpenAI: {
      id: 'openai',
      providerType: 'Brain / Text API',
      displayName: 'OpenAI',
      documentationUrl: 'https://platform.openai.com/docs',
      notes: 'Use this placeholder to note the credentials you expect to wire back in later.',
      fields: [
        { key: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Paste a project or service API key.' },
        { key: 'baseUrl', label: 'Base URL', inputType: 'text', helpText: 'Optional custom endpoint for gateways or proxies.' }
      ]
    },
    OpenRouter: {
      id: 'openrouter',
      providerType: 'Brain / Text API',
      displayName: 'OpenRouter',
      documentationUrl: 'https://openrouter.ai/docs',
      notes: 'Helpful when you want one endpoint for multiple models.',
      fields: [
        { key: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'OpenRouter API key.' },
        { key: 'model', label: 'Model Alias', inputType: 'text', helpText: 'Store the default routing target here.' }
      ]
    },
    Anthropic: {
      id: 'anthropic',
      providerType: 'Brain / Text API',
      displayName: 'Anthropic',
      documentationUrl: 'https://docs.anthropic.com',
      notes: 'Keep account and model notes here until the real settings page replaces this flow.',
      fields: [
        { key: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Claude API key.' }
      ]
    },
    'Google AI Studio': {
      id: 'google-ai-studio',
      providerType: 'Brain / Text API',
      displayName: 'Google AI Studio',
      documentationUrl: 'https://ai.google.dev',
      notes: 'Use this slot for Gemini credentials or endpoint notes.',
      fields: [
        { key: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Gemini API key.' }
      ]
    },
    Runway: {
      id: 'runway',
      providerType: 'Video API',
      displayName: 'Runway',
      documentationUrl: 'https://help.runwayml.com/hc/en-us',
      notes: 'Track the account used for generation and any environment notes.',
      fields: [
        { key: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Runway API key.' }
      ]
    },
    Pika: {
      id: 'pika',
      providerType: 'Video API',
      displayName: 'Pika',
      documentationUrl: 'https://pika.art',
      notes: 'Add connection details here if this becomes the active renderer again.',
      fields: [
        { key: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Pika access token or API key.' }
      ]
    },
    Synthesia: {
      id: 'synthesia',
      providerType: 'Video API',
      displayName: 'Synthesia',
      documentationUrl: 'https://docs.synthesia.io',
      notes: 'Useful for avatar-based workflows and enterprise teams.',
      fields: [
        { key: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'Synthesia API key.' }
      ]
    },
    HeyGen: {
      id: 'heygen',
      providerType: 'Video API',
      displayName: 'HeyGen',
      documentationUrl: 'https://docs.heygen.com',
      notes: 'Placeholder only. No live API call is performed from this page.',
      fields: [
        { key: 'apiKey', label: 'API Key', inputType: 'password', helpText: 'HeyGen API key.' }
      ]
    },
    'YouTube Data API': {
      id: 'youtube-data-api',
      providerType: 'Upload API',
      displayName: 'YouTube Data API',
      documentationUrl: 'https://developers.google.com/youtube/v3',
      notes: 'Store OAuth client details for future migration work.',
      fields: [
        { key: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'Google OAuth client ID.' },
        { key: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'Google OAuth client secret.' }
      ]
    },
    'TikTok API': {
      id: 'tiktok-api',
      providerType: 'Upload API',
      displayName: 'TikTok API',
      documentationUrl: 'https://developers.tiktok.com',
      notes: 'Use these notes to capture app approval or token requirements.',
      fields: [
        { key: 'clientKey', label: 'Client Key', inputType: 'text', helpText: 'TikTok client key.' },
        { key: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'TikTok client secret.' }
      ]
    },
    'Meta Graph API': {
      id: 'meta-graph-api',
      providerType: 'Upload API',
      displayName: 'Meta Graph API',
      documentationUrl: 'https://developers.facebook.com/docs/graph-api',
      notes: 'Handy for Instagram and Facebook publishing details.',
      fields: [
        { key: 'appId', label: 'App ID', inputType: 'text', helpText: 'Meta app identifier.' },
        { key: 'appSecret', label: 'App Secret', inputType: 'password', helpText: 'Meta app secret.' }
      ]
    },
    'LinkedIn API': {
      id: 'linkedin-api',
      providerType: 'Upload API',
      displayName: 'LinkedIn API',
      documentationUrl: 'https://learn.microsoft.com/linkedin/',
      notes: 'Keep LinkedIn publishing credentials or org URNs here for now.',
      fields: [
        { key: 'clientId', label: 'Client ID', inputType: 'text', helpText: 'LinkedIn app client ID.' },
        { key: 'clientSecret', label: 'Client Secret', inputType: 'password', helpText: 'LinkedIn app client secret.' }
      ]
    },
    'Google Drive': {
      id: 'google-drive',
      providerType: 'Storage API',
      displayName: 'Google Drive',
      documentationUrl: 'https://developers.google.com/drive',
      notes: 'Use the folder ID field to remember where generated assets should land.',
      fields: [
        { key: 'folderId', label: 'Folder ID', inputType: 'text', helpText: 'Destination Google Drive folder ID.' }
      ]
    },
    Dropbox: {
      id: 'dropbox',
      providerType: 'Storage API',
      displayName: 'Dropbox',
      documentationUrl: 'https://www.dropbox.com/developers',
      notes: 'Capture access and destination details locally in this placeholder.',
      fields: [
        { key: 'accessToken', label: 'Access Token', inputType: 'password', helpText: 'Dropbox access token.' }
      ]
    },
    OneDrive: {
      id: 'onedrive',
      providerType: 'Storage API',
      displayName: 'OneDrive',
      documentationUrl: 'https://learn.microsoft.com/graph/onedrive-concept-overview',
      notes: 'Use this to stage Microsoft Graph details until the real settings page takes over.',
      fields: [
        { key: 'driveId', label: 'Drive ID', inputType: 'text', helpText: 'OneDrive or SharePoint drive ID.' }
      ]
    },
    'Local Filesystem': {
      id: 'local-filesystem',
      providerType: 'Storage API',
      displayName: 'Local Filesystem',
      notes: 'No remote credential is required for local output paths.',
      fields: []
    }
  };

  protected readonly credentialDrafts: Record<string, string> = {};

  protected status = 'Legacy API management controls are now a local-only placeholder.';
  protected lastSavedAt: string | null = null;

  protected get selectedRequirements(): ProviderRequirement[] {
    const seen = new Set<string>();

    return Object.values(this.providers)
      .filter((provider) => {
        if (seen.has(provider)) {
          return false;
        }

        seen.add(provider);
        return true;
      })
      .map((provider) => this.requirementCatalog[provider])
      .filter((requirement): requirement is ProviderRequirement => Boolean(requirement));
  }

  protected get providerMap(): Array<{ label: string; provider: string }> {
    return [
      { label: 'Brain Agent', provider: this.providers.textProvider },
      { label: 'Script Agent', provider: this.providers.textProvider },
      { label: 'Video Agent', provider: this.providers.videoProvider },
      { label: 'Upload Agent', provider: this.providers.uploadProvider },
      { label: 'Storage Layer', provider: this.providers.storageProvider }
    ];
  }

  protected saveProviders(): void {
    this.lastSavedAt = new Date().toLocaleTimeString();
    this.status = 'Selections and credentials were saved locally in this placeholder view only.';
  }

  protected isCredentialSaved(requirement: ProviderRequirement, field: RequirementField): boolean {
    return this.credentialValue(requirement, field).trim().length > 0;
  }

  protected credentialValue(requirement: ProviderRequirement, field: RequirementField): string {
    return this.credentialDrafts[this.credentialKey(requirement, field)] ?? '';
  }

  protected updateCredential(requirement: ProviderRequirement, field: RequirementField, value: string): void {
    this.credentialDrafts[this.credentialKey(requirement, field)] = value;
  }

  private credentialKey(requirement: ProviderRequirement, field: RequirementField): string {
    return `${requirement.id}.${field.key}`;
  }
}
