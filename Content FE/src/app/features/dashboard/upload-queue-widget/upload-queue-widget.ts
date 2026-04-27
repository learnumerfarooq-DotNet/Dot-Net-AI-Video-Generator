import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-upload-queue-widget',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload-queue-widget.html',
  styleUrl: './upload-queue-widget.css'
})
export class UploadQueueWidgetComponent {
  
  upcomingUploads = computed(() => [
    { title: 'Why AI is taking over', platform: 'YouTube', time: 'In 2 hours', status: 'Scheduled' },
    { title: 'Tech Trends 2026', platform: 'TikTok', time: 'In 5 hours', status: 'Waiting' },
    { title: 'My Coding Setup', platform: 'Instagram', time: 'Tomorrow 10 AM', status: 'Waiting' }
  ]);

  getPlatformColor(platform: string): string {
    switch (platform) {
      case 'YouTube': return 'text-red-500 bg-red-500/10 border-red-500/20';
      case 'TikTok': return 'text-cyan-500 bg-cyan-500/10 border-cyan-500/20';
      case 'Instagram': return 'text-pink-500 bg-pink-500/10 border-pink-500/20';
      default: return 'text-gray-500 bg-gray-500/10 border-gray-500/20';
    }
  }
}
