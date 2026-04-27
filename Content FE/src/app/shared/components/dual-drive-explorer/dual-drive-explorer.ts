import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { DriveExplorerComponent } from '../../../features/drive/drive-explorer';

@Component({
  selector: 'app-dual-drive-explorer',
  standalone: true,
  imports: [CommonModule, DriveExplorerComponent],
  templateUrl: './dual-drive-explorer.html',
  styleUrl: './dual-drive-explorer.css'
})
export class DualDriveExplorerComponent {
  @Input() leftTitle = 'Raw Video';
  @Input() leftFolderName = 'Raw Video';
  
  @Input() rightTitle = 'Upload Video';
  @Input() rightFolderName = 'Upload Video';
}
