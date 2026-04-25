import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../core/store/content-factory.store';
import { UsagePoint, UsageSeries } from '../../core/models/content-factory.models';

@Component({
  selector: 'app-dashboard-page',
  imports: [CommonModule],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.css'
})
export class DashboardPageComponent {
  protected readonly store = inject(ContentFactoryStore);

  protected maxUsage(series: UsageSeries): number {
    return Math.max(...series.points.map((point) => point.tokensIn + point.tokensOut), 1);
  }

  protected usageHeight(point: UsagePoint, series: UsageSeries): number {
    return ((point.tokensIn + point.tokensOut) / this.maxUsage(series)) * 100;
  }

  protected latestUsagePoint(series: UsageSeries): UsagePoint | null {
    return series.points[series.points.length - 1] ?? null;
  }
}
