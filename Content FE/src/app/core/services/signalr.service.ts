import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { API_BASE } from '../constants/api-endpoints';

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private hubConnection: signalR.HubConnection | null = null;
  
  // Real-time signals that components can react to
  videoStageChanged = signal<{ videoId: string, stage: string } | null>(null);
  agentRunStarted = signal<{ runId: string, agentKey: string } | null>(null);
  agentRunCompleted = signal<{ runId: string, agentKey: string, status: string } | null>(null);
  memoryAdded = signal<{ memoryId: string } | null>(null);

  connectionStatus = signal<'Disconnected' | 'Connecting' | 'Connected' | 'Error'>('Disconnected');

  constructor() {
    this.startConnection();
  }

  private startConnection(): void {
    this.connectionStatus.set('Connecting');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE}/hubs/studio`, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => {
        this.connectionStatus.set('Connected');
        this.registerHandlers();
      })
      .catch(err => {
        console.error('Error while starting SignalR connection: ' + err);
        this.connectionStatus.set('Error');
      });
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('OnVideoStageChanged', (videoId: string, stage: string) => {
      this.videoStageChanged.set({ videoId, stage });
    });

    this.hubConnection.on('OnAgentRunStarted', (runId: string, agentKey: string) => {
      this.agentRunStarted.set({ runId, agentKey });
    });

    this.hubConnection.on('OnAgentRunCompleted', (runId: string, agentKey: string, status: string) => {
      this.agentRunCompleted.set({ runId, agentKey, status });
    });

    this.hubConnection.on('OnMemoryAdded', (memoryId: string) => {
      this.memoryAdded.set({ memoryId });
    });
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.connectionStatus.set('Disconnected');
    }
  }
}
