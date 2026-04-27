import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { DashboardStore } from '../store/dashboard.store';

import { PipelineStatusCardComponent } from '../pipeline-status-card/pipeline-status-card';
import { BrainStatusWidgetComponent } from '../brain-status-widget/brain-status-widget';
import { AgentHealthGridComponent } from '../agent-health-grid/agent-health-grid';
import { AnalyticsSummaryCardComponent } from '../analytics-summary-card/analytics-summary-card';
import { TrendRadarWidgetComponent } from '../trend-radar-widget/trend-radar-widget';
import { UploadQueueWidgetComponent } from '../upload-queue-widget/upload-queue-widget';
import { ErrorMonitorWidgetComponent } from '../error-monitor-widget/error-monitor-widget';

@Component({
  selector: 'app-dashboard-overview',
  imports: [
    CommonModule,
    PipelineStatusCardComponent,
    BrainStatusWidgetComponent,
    AgentHealthGridComponent,
    AnalyticsSummaryCardComponent,
    TrendRadarWidgetComponent,
    UploadQueueWidgetComponent,
    ErrorMonitorWidgetComponent
  ],
  templateUrl: './dashboard-overview.html',
  styleUrl: './dashboard-overview.css'
})
export class DashboardOverviewComponent {
  protected readonly dashboardStore = inject(DashboardStore);
}
