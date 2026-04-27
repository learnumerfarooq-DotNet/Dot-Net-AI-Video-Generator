import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-error-monitor-widget',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './error-monitor-widget.html',
  styleUrl: './error-monitor-widget.css'
})
export class ErrorMonitorWidgetComponent {
  
  errors24h = computed(() => 3);
  circuitBreakerStatus = computed(() => 'Closed');
  
  recentErrors = computed(() => [
    { component: 'YouTube API', message: 'Rate limit exceeded', time: '10m ago' },
    { component: 'FFmpeg Node', message: 'OOM Error during render', time: '2h ago' },
    { component: 'Trend Agent', message: 'OpenRouter timeout', time: '5h ago' }
  ]);

}
