import { Injectable, signal, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { API_BASE } from '../constants/api-endpoints';
import { PipelineStore } from '../store/pipeline.store';
import { AgentsStore } from '../../features/agents/store/agents.store';
import { MemoryStore } from '../../features/memory/store/memory.store';
import { SchedulerStore } from '../../features/scheduler/store/scheduler.store';
import { ContentFactoryStore } from '../store/content-factory.store';

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private hubConnection: signalR.HubConnection | null = null;
  
  private readonly rootStore = inject(ContentFactoryStore);
  private readonly pipelineStore = inject(PipelineStore);
  private readonly agentsStore = inject(AgentsStore);
  private readonly memoryStore = inject(MemoryStore);
  private readonly schedulerStore = inject(SchedulerStore);
  
  // Backward compatibility signals
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
        this.joinInitialGroups();
      })
      .catch(err => {
        console.error('Error while starting SignalR connection: ' + err);
        this.connectionStatus.set('Error');
      });
  }

  private joinInitialGroups() {
    if (!this.hubConnection) return;
    this.hubConnection.invoke('SubscribeToBrain');
    this.hubConnection.invoke('SubscribeToPipeline');
    this.hubConnection.invoke('SubscribeToPublishing');
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    // --- Brain Events ---
    this.hubConnection.on('BrainTickCompleted', (payload) => {
       console.log('Brain Tick:', payload);
    });

    this.hubConnection.on('BrainStatusChanged', (payload) => {
       this.rootStore.setStatus(payload.status);
    });

    this.hubConnection.on('GlobalMemorySynced', (payload) => {
       this.memoryStore.loadGlobalMemory();
       this.memoryAdded.set({ memoryId: 'sync' });
    });

    this.hubConnection.on('CircuitBreakerStateChanged', (payload) => {
       // Notify UI of agent circuit breaker
    });

    // --- Pipeline Events ---
    this.hubConnection.on('JobStarted', (payload) => {
      this.pipelineStore.handleJobStarted(payload);
    });

    this.hubConnection.on('StageCompleted', (payload) => {
      this.pipelineStore.handleStageCompleted(payload.jobId, payload.stageName, payload.progress);
      this.videoStageChanged.set({ videoId: payload.jobId, stage: payload.stageName });
    });

    this.hubConnection.on('ProgressUpdated', (payload) => {
      this.pipelineStore.handleProgressUpdated(payload.jobId, payload.stage, payload.percent);
    });

    this.hubConnection.on('JobCompleted', (payload) => {
      this.pipelineStore.handleJobCompleted(payload.jobId);
    });

    this.hubConnection.on('JobFailed', (payload) => {
      this.pipelineStore.handleJobFailed(payload.jobId, payload.error, payload.retryCount);
    });

    // --- Agent Events ---
    this.hubConnection.on('AgentDispatched', (payload) => {
       // Update agent queue in UI
    });

    this.hubConnection.on('AgentRunStarted', (payload) => {
      this.agentsStore.handleRunStarted(payload);
      this.agentRunStarted.set({ runId: payload.runId, agentKey: payload.agentKey });
    });

    this.hubConnection.on('AgentRunCompleted', (payload) => {
      this.agentsStore.handleRunCompleted(payload);
      this.agentRunCompleted.set({ runId: payload.runId, agentKey: payload.agentKey, status: payload.status });
    });

    this.hubConnection.on('AgentHealthChanged', (payload) => {
      this.agentsStore.handleHealthChanged(payload);
    });

    this.hubConnection.on('AgentChatResponse', (payload) => {
      this.agentsStore.handleChatResponse(payload);
    });

    this.hubConnection.on('AgentChatStreamChunk', (payload) => {
      this.agentsStore.handleChatStreamChunk(payload);
    });

    // --- Content Events ---
    this.hubConnection.on('ScriptGenerated', (payload) => {
       // Show notification
    });

    this.hubConnection.on('VideoEdited', (payload) => {
       // Show notification
    });

    // --- Publishing Events ---
    this.hubConnection.on('UploadStarted', (payload) => {
       // Update scheduler/queue
    });

    this.hubConnection.on('UploadProgress', (payload) => {
       // Update scheduler/queue
    });

    this.hubConnection.on('UploadCompleted', (payload) => {
       // Update scheduler/queue
    });

    this.hubConnection.on('UploadFailed', (payload) => {
       // Show error alert
    });

    // --- System Events ---
    this.hubConnection.on('DriveFileDetected', (payload) => {
       // Trigger drive explorer refresh
    });
  }

  public subscribeToAgent(agentKey: string) {
    this.hubConnection?.invoke('SubscribeToAgent', agentKey);
  }

  public unsubscribeFromAgent(agentKey: string) {
    this.hubConnection?.invoke('UnsubscribeFromAgent', agentKey);
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.connectionStatus.set('Disconnected');
    }
  }
}
