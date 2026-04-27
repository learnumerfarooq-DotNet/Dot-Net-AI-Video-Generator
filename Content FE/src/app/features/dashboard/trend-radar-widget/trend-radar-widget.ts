import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-trend-radar-widget',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './trend-radar-widget.html',
  styleUrl: './trend-radar-widget.css'
})
export class TrendRadarWidgetComponent {
  
  trendingTopics = computed(() => [
    { topic: 'AI Agents in 2026', heat: 98, direction: 'up' },
    { topic: 'Quantum Computing basics', heat: 85, direction: 'up' },
    { topic: 'Retro UI Design', heat: 72, direction: 'stable' },
    { topic: 'Angular Signals Guide', heat: 88, direction: 'up' },
    { topic: 'Crypto predictions', heat: 45, direction: 'down' }
  ]);

  getHeatColor(heat: number): string {
    if (heat >= 90) return 'text-red-500 bg-red-500/10 border-red-500/20';
    if (heat >= 75) return 'text-orange-500 bg-orange-500/10 border-orange-500/20';
    if (heat >= 50) return 'text-yellow-500 bg-yellow-500/10 border-yellow-500/20';
    return 'text-blue-500 bg-blue-500/10 border-blue-500/20';
  }
}
