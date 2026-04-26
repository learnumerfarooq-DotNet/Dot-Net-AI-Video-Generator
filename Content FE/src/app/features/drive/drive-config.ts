import { CommonModule } from '@angular/common';
import { Component, inject, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ContentFactoryStore } from '../../core/store/content-factory.store';
import { DriveStore } from './store/drive.store';

@Component({
  selector: 'app-drive-config',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './drive-config.html',
  styleUrl: './drive-config.css'
})
export class DriveConfigComponent {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly driveStore = inject(DriveStore);
  private readonly router = inject(Router);
  private lastValidatedConfigKey = '';

  config = {
    clientId: '',
    clientSecret: '',
    refreshToken: '',
    rootFolderId: ''
  };

  constructor() {
    // Populate form from store if available
    effect(() => {
      const saved = this.driveStore.driveConfig();
      if (saved) {
        this.config = {
          clientId: saved.clientId || this.config.clientId,
          clientSecret: saved.clientSecret || this.config.clientSecret,
          refreshToken: saved.refreshToken || '',
          rootFolderId: saved.rootFolderId || ''
        };

        const key = `${saved.clientId}|${saved.refreshToken}|${saved.rootFolderId}`;
        if (saved.clientId?.trim() && saved.refreshToken?.trim() && key !== this.lastValidatedConfigKey) {
          this.lastValidatedConfigKey = key;
          void this.driveStore.testDriveConnection();
        }
      }
    });
  }

  get redirectUri(): string {
    return `${window.location.origin}/drive/oauth/callback`;
  }

  startOAuth() {
    const clientId = this.config.clientId.trim();
    if (!clientId) {
      alert('Please enter a Client ID first.');
      return;
    }

    const redirectUri = encodeURIComponent(this.redirectUri);
    const scope = encodeURIComponent('https://www.googleapis.com/auth/drive');
    const authUrl =
      `https://accounts.google.com/o/oauth2/auth` +
      `?client_id=${clientId}` +
      `&redirect_uri=${redirectUri}` +
      `&response_type=code` +
      `&scope=${scope}` +
      `&access_type=offline` +
      `&prompt=consent`;

    window.location.href = authUrl;
  }

  copyUri() {
    navigator.clipboard.writeText(this.redirectUri).then(() => {
      // Visual feedback could be added here
    });
  }

  async saveConfig() {
    await this.driveStore.saveDriveConfig(this.config);
    this.router.navigate(['/drive/explorer']);
    this.store.setSideTab('drive-explorer');
  }
}
