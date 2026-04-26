import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ContentFactoryStore } from '../../core/store/content-factory.store';
import { DriveStore } from './store/drive.store';

@Component({
  selector: 'app-drive-explorer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './drive-explorer.html',
  styleUrl: './drive-explorer.css'
})
export class DriveExplorerComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly driveStore = inject(DriveStore);
  private readonly router = inject(Router);

  showNewFolderModal = false;
  showUploadModal = false;
  newFolderName = '';
  selectedFile: File | null = null;

  ngOnInit() {
    if (!this.driveStore.isDriveConfigured()) {
      this.router.navigate(['/drive/config']);
      this.store.setSideTab('drive-config');
      return;
    }
    this.driveStore.loadDriveFiles();
  }

  getFileIcon(type: string): string {
    if (!type) return 'fa-file';
    const lowerType = type.toLowerCase();
    
    if (lowerType.includes('folder')) return 'fa-folder';
    if (lowerType.includes('video')) return 'fa-file-video';
    if (lowerType.includes('image')) return 'fa-file-image';
    if (lowerType.includes('pdf')) return 'fa-file-pdf';
    if (lowerType.includes('spreadsheet') || lowerType.includes('sheet')) return 'fa-file-excel';
    if (lowerType.includes('presentation') || lowerType.includes('slides')) return 'fa-file-powerpoint';
    if (lowerType.includes('google-doc') || lowerType.includes('document')) return 'fa-file-lines';
    if (lowerType.includes('json') || lowerType.includes('code') || lowerType.includes('script')) return 'fa-file-code';
    
    return 'fa-file';
  }

  async confirmCreateFolder() {
    if (this.newFolderName.trim()) {
      await this.driveStore.createDriveFolder(this.newFolderName.trim());
      this.newFolderName = '';
      this.showNewFolderModal = false;
    }
  }

  async confirmUpload() {
    if (this.selectedFile) {
      await this.driveStore.uploadDriveFile(this.selectedFile);
      this.selectedFile = null;
      this.showUploadModal = false;
    }
  }

  onItemClick(item: any) {
    const type = item.type || item.Type;
    const id = item.id || item.Id;
    const name = item.name || item.Name;
    
    if (type === 'folder') {
      this.driveStore.navigateToFolder(id, name);
    }
  }

  onBreadcrumbClick(crumb: { id: string | null, name: string }) {
    this.driveStore.navigateToFolder(crumb.id, crumb.name);
  }

  onFileSelected(event: any) {
    const file = event.target.files?.[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  downloadFile(event: Event, item: any) {
    event.stopPropagation();
    this.driveStore.downloadDriveFile(item);
  }

  goBack() {
    const breadcrumbs = this.driveStore.breadcrumbs();
    if (breadcrumbs.length > 1) {
      const parent = breadcrumbs[breadcrumbs.length - 2];
      this.driveStore.navigateToFolder(parent.id, parent.name);
    }
  }
}
