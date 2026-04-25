# Bulk-generate generic agent components
$agents = @(
  @{dir="trend-agent"; selector="app-trend-agent"; class="TrendAgentComponent"},
  @{dir="script-agent"; selector="app-script-agent"; class="ScriptAgentComponent"},
  @{dir="video-generation-agent"; selector="app-video-generation-agent"; class="VideoGenerationAgentComponent"},
  @{dir="shorts-agent-1"; selector="app-shorts-agent-1"; class="ShortsAgent1Component"},
  @{dir="shorts-agent-2"; selector="app-shorts-agent-2"; class="ShortsAgent2Component"}
)

$tsTemplate = @'
import {{ '{' }} CommonModule {{ '}' }} from '@angular/common';
import {{ '{' }} Component, inject {{ '}' }} from '@angular/core';
import {{ '{' }} ReactiveFormsModule {{ '}' }} from '@angular/forms';
import {{ '{' }} ContentFactoryStore {{ '}' }} from '../../../core/store/content-factory.store';
import {{ '{' }} AgentChatComponent {{ '}' }} from '../../../shared/agent-chat/agent-chat';

@Component({{
  selector: '{selector}',
  imports: [CommonModule, ReactiveFormsModule, AgentChatComponent],
  templateUrl: './{dir}.html',
  styleUrl: './{dir}.css'
}})
export class {class} {{
  protected readonly store = inject(ContentFactoryStore);
}}
'@

$htmlTemplate = @'
<ng-container *ngIf="store.activeAgent() as agent">
  <div class="agent-workspace">
    <header class="agent-header">
      <div class="agent-info">
        <div class="agent-icon"><i class="fa-solid fa-robot"></i></div>
        <div class="agent-name-stack">
          <h2>{{ agent.name }}</h2>
          <div class="agent-status">
            <span class="status-dot" [class.active]="agent.isConnected"></span>
            {{ agent.isConnected ? 'Connected' : 'Disconnected' }} &bull; {{ agent.category }}
          </div>
        </div>
      </div>
      <button type="button" class="ghost-button"
              (click)="store.setSection('settings'); store.setSideTab('settings-' + agent.key)">
        <i class="fa-solid fa-sliders"></i><span>Configure</span>
      </button>
    </header>
    <div class="agent-layout">
      <app-agent-chat [agent]="agent"></app-agent-chat>
      <aside class="detail-section">
        <section class="detail-card">
          <h3><i class="fa-solid fa-microchip"></i> Intelligence</h3>
          <div class="item-info">
            <div class="item-title">{{ agent.providerName || 'N/A' }}</div>
            <div class="item-meta">{{ agent.modelName || 'No model' }}</div>
          </div>
        </section>
        <section class="detail-card">
          <h3><i class="fa-solid fa-clock-rotate-left"></i> Recent Runs</h3>
          <div class="list-stack">
            <div class="list-item" *ngFor="let run of agent.recentRuns">
              <div class="item-info">
                <div class="item-title">{{ run.title }}</div>
                <div class="item-meta">{{ run.status }}</div>
              </div>
              <span class="item-meta">{{ run.queuedAt | date:'h:mm a' }}</span>
            </div>
          </div>
        </section>
      </aside>
    </div>
  </div>
</ng-container>
'@

$base = "n:\MY AI Project For Video\src\ai-content-dashboard\src\app\features\agents"

foreach ($a in $agents) {
  $tsContent = $tsTemplate -replace '{selector}', $a.selector -replace '{dir}', $a.dir -replace '{class}', $a.class
  Set-Content -Path "$base\$($a.dir)\$($a.dir).ts" -Value $tsContent -Encoding UTF8
  Set-Content -Path "$base\$($a.dir)\$($a.dir).html" -Value $htmlTemplate -Encoding UTF8
  Write-Host "Written: $($a.dir)"
}

Write-Host "Done!"
