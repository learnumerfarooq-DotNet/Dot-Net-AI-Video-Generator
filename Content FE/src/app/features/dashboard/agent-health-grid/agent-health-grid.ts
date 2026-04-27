import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-agent-health-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './agent-health-grid.html',
  styleUrl: './agent-health-grid.css'
})
export class AgentHealthGridComponent {
  
  agents = computed(() => [
    { name: 'Trend Agent', status: 'Healthy', lastRun: '2m ago', successRate: '98.5%', runCount: 1245 },
    { name: 'Script Agent', status: 'Healthy', lastRun: '5m ago', successRate: '99.1%', runCount: 890 },
    { name: 'Edit Agent', status: 'Degraded', lastRun: '1h ago', successRate: '92.4%', runCount: 540 },
    { name: 'Shorts Agent', status: 'Healthy', lastRun: '15m ago', successRate: '97.2%', runCount: 610 },
    { name: 'Short Edit', status: 'Healthy', lastRun: '20m ago', successRate: '98.8%', runCount: 590 },
    { name: 'Upload Agent', status: 'Healthy', lastRun: '30m ago', successRate: '99.5%', runCount: 420 },
    { name: 'Analytics', status: 'Healthy', lastRun: '4h ago', successRate: '100%', runCount: 120 },
    { name: 'Main Brain', status: 'Healthy', lastRun: '30s ago', successRate: '99.9%', runCount: 12456 }
  ]);

  getStatusClass(status: string): string {
    return status === 'Healthy' ? 'bg-success/10 text-success' : 'bg-danger/10 text-danger';
  }

  getAgentIcon(name: string): string {
    if (name.includes('Brain')) return 'fa-solid fa-brain';
    if (name.includes('Trend')) return 'fa-solid fa-bolt';
    if (name.includes('Script')) return 'fa-solid fa-pen';
    if (name.includes('Edit')) return 'fa-solid fa-film';
    if (name.includes('YouTube')) return 'fa-brands fa-youtube';
    return 'fa-solid fa-robot';
  }
}
