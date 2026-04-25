import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { UsagePoint, UsageSeries } from '../../../core/models/content-factory.models';

@Component({
  selector: 'app-dashboard-usage',
  imports: [CommonModule],
  templateUrl: './dashboard-usage.html',
  styleUrl: './dashboard-usage.css'
})
export class DashboardUsageComponent {
  protected readonly store = inject(ContentFactoryStore);

  maxUsage(series: UsageSeries): number {
    return Math.max(...series.points.map((p) => p.tokensIn + p.tokensOut), 1);
  }

  usageHeight(point: UsagePoint, series: UsageSeries): number {
    return ((point.tokensIn + point.tokensOut) / this.maxUsage(series)) * 100;
  }

  latestPoint(series: UsageSeries): UsagePoint | null {
    return series.points[series.points.length - 1] ?? null;
  }
}
