import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ContentFactoryStore } from '../../core/store/content-factory.store';

@Component({
  selector: 'app-drive-explorer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './drive-explorer.html',
  styleUrl: './drive-explorer.css'
})
export class DriveExplorerComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  private readonly router = inject(Router);

  ngOnInit() {
    if (!this.store.isDriveConfigured()) {
      this.router.navigate(['/drive/config']);
      this.store.setSideTab('drive-config');
      return;
    }
    this.store.loadDriveFiles();
  }

  getFileIcon(type: string): string {
    switch (type) {
      case 'folder': return 'fa-folder';
      case 'video': return 'fa-file-video';
      case 'image': return 'fa-file-image';
      default: return 'fa-file';
    }
  }

  async createFolder() {
    const name = prompt('Enter folder name:');
    if (name && name.trim()) {
      await this.store.createDriveFolder(name.trim());
    }
  }
}
