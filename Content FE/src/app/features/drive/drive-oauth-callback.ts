import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ContentFactoryStore } from '../../core/store/content-factory.store';
import { DriveStore } from './store/drive.store';

@Component({
  selector: 'app-drive-oauth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="oauth-callback">
      <div class="callback-card">
        @if (processing) {
          <div class="spinner"></div>
          <h2>Connecting to Google Drive...</h2>
          <p>Exchanging authorization code for access tokens.</p>
        }
        @if (success) {
          <div class="success-icon">
            <i class="fa-solid fa-check-circle"></i>
          </div>
          <h2>Google Drive Connected!</h2>
          <p>Your refresh token has been saved. Redirecting to Drive config...</p>
        }
        @if (error) {
          <div class="error-icon">
            <i class="fa-solid fa-exclamation-triangle"></i>
          </div>
          <h2>Connection Failed</h2>
          <p class="error-text">{{ error }}</p>
          <button class="primary-button" (click)="goBack()">
            <i class="fa-solid fa-arrow-left"></i> Back to Drive Config
          </button>
        }
      </div>
    </div>
  `,
  styles: [`
    .oauth-callback {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 60vh;
      padding: 40px;
    }
    .callback-card {
      text-align: center;
      background: var(--surface-strong);
      border: 1px solid var(--border);
      border-radius: 16px;
      padding: 48px;
      max-width: 480px;
      width: 100%;
      box-shadow: var(--shadow-soft);
    }
    .callback-card h2 {
      margin: 16px 0 8px;
      color: var(--text);
      font-size: 1.3rem;
    }
    .callback-card p {
      color: var(--text-muted);
      font-size: 0.95rem;
      line-height: 1.6;
    }
    .spinner {
      width: 48px;
      height: 48px;
      border: 4px solid var(--border);
      border-top-color: var(--primary);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin: 0 auto 16px;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
    .success-icon {
      font-size: 3rem;
      color: var(--success);
    }
    .error-icon {
      font-size: 3rem;
      color: var(--danger);
    }
    .error-text {
      color: var(--danger) !important;
      background: rgba(255,59,48,0.08);
      border-radius: 8px;
      padding: 12px;
      font-family: monospace;
      font-size: 0.85rem !important;
      word-break: break-all;
    }
  `]
})
export class DriveOAuthCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly store = inject(ContentFactoryStore);
  private readonly driveStore = inject(DriveStore);

  processing = true;
  success = false;
  error = '';

  async ngOnInit() {
    const code = this.route.snapshot.queryParamMap.get('code');
    const errorParam = this.route.snapshot.queryParamMap.get('error');

    if (errorParam) {
      this.processing = false;
      this.error = `Google returned error: ${errorParam}`;
      return;
    }

    if (!code) {
      this.processing = false;
      this.error = 'No authorization code received from Google.';
      return;
    }

    try {
      const result = await firstValueFrom(
        this.http.post<{ refreshToken: string; accessToken: string; expiresIn: number }>(
          'http://localhost:5039/api/drive/oauth/exchange',
          { code, redirectUri: `${window.location.origin}/drive/oauth/callback` }
        )
      );

      // Save the tokens into the drive config
      const currentConfig = this.driveStore.driveConfig?.() ?? { clientId: '', clientSecret: '', rootFolderId: '' };
      await this.driveStore.saveDriveConfig({
        ...currentConfig,
        refreshToken: result.refreshToken
      });

      this.processing = false;
      this.success = true;

      // Redirect after 2 seconds
      setTimeout(() => {
        this.router.navigate(['/drive/drive-config']);
        this.store.setSideTab('drive-config');
      }, 2000);
    } catch (err: any) {
      this.processing = false;
      this.error = err?.error?.message || err?.message || 'Failed to exchange authorization code.';
    }
  }

  goBack() {
    this.router.navigate(['/drive/drive-config']);
    this.store.setSideTab('drive-config');
  }
}
