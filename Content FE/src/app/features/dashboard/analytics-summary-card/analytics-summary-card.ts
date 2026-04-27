import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-analytics-summary-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics-summary-card.html',
  styleUrl: './analytics-summary-card.css'
})
export class AnalyticsSummaryCardComponent {
  
  views = computed(() => '1.2M');
  viewsTrend = computed(() => '+12%');
  likes = computed(() => '85K');
  ctr = computed(() => '8.4%');
  watchTime = computed(() => '4:20');
  engagementRate = computed(() => '11.2%');
  bestUploadHour = computed(() => '6 PM UTC+5');
  topPlatform = computed(() => 'YouTube Shorts');
  growth = computed(() => '+15%');

}
