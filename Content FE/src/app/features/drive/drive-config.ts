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
  
  config = {
    clientId: '',
    clientSecret: '',
    refreshToken: '',
    rootFolderId: '',
    pollingInterval: 30,
    autoCreateFolders: true
  };

  showSecrets = false;

  constructor() {
    effect(() => {
      const saved = this.driveStore.driveConfig();
      if (saved) {
        this.config = {
          clientId: saved.clientId || '',
          clientSecret: saved.clientSecret || '',
          refreshToken: saved.refreshToken || '',
          rootFolderId: saved.rootFolderId || '',
          pollingInterval: saved.pollingInterval || 30,
          autoCreateFolders: saved.autoCreateFolders ?? true
        };
      }
    });
  }

  get redirectUri(): string {
    return `${window.location.origin}/drive/oauth/callback`;
  }

  startOAuth() {
    if (!this.config.clientId.trim()) {
      alert('Please enter a Client ID first.');
      return;
    }

    const clientId = this.config.clientId.trim();
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

  async saveConfig() {
    await this.driveStore.saveDriveConfig(this.config);
  }

  async disconnect() {
    if (confirm('Are you sure you want to disconnect Google Drive? This will clear your tokens.')) {
      await this.driveStore.saveDriveConfig({ ...this.config, refreshToken: '' });
    }
  }

  copyUri() {
    navigator.clipboard.writeText(this.redirectUri);
  }
}
