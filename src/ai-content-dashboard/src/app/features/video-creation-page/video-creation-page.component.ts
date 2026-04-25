import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

type VideoPlatform = 'youtube' | 'tiktok' | 'instagram' | 'linkedin';
type VideoFormat = 'short' | 'longform' | 'carousel';

type VideoTaskDraft = {
  topic: string;
  platform: VideoPlatform;
  format: VideoFormat;
  audience: string;
  goal: string;
  autoSaveLocalMemory: boolean;
};

type AgentResult = {
  agentName: string;
  summary: string;
};

type VideoRunPreview = {
  topic: string;
  createdAt: string;
  agentResults: AgentResult[];
};

type ReadyVideoPreview = {
  topic: string;
  platform: string;
  format: string;
};

@Component({
  selector: 'app-video-creation-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './video-creation-page.component.html',
  styleUrl: './video-creation-page.component.css'
})
export class VideoCreationPageComponent {
  protected readonly task: VideoTaskDraft = {
    topic: '',
    platform: 'youtube',
    format: 'short',
    audience: '',
    goal: '',
    autoSaveLocalMemory: true
  };

  protected latestRun: VideoRunPreview | null = null;
  protected readonly readyVideoItems: ReadyVideoPreview[] = [];
  protected running = false;
  protected status = 'Legacy video orchestration was removed from the store. This page now creates local previews only.';

  protected get readyItems(): number {
    return this.readyVideoItems.length;
  }

  protected runTask(): void {
    const topic = this.task.topic.trim();

    if (!topic) {
      this.status = 'Enter a topic to create a local preview.';
      return;
    }

    this.running = true;

    const createdAt = new Date().toLocaleTimeString();
    const audience = this.task.audience.trim() || 'a general audience';
    const goal = this.task.goal.trim() || `Publish a ${this.readableFormat(this.task.format).toLowerCase()} video on ${this.readablePlatform(this.task.platform)}.`;

    this.latestRun = {
      topic,
      createdAt,
      agentResults: [
        {
          agentName: 'Research Agent',
          summary: `Prepared a placeholder content brief for ${topic} aimed at ${audience}.`
        },
        {
          agentName: 'Script Agent',
          summary: `Drafted a simple hook, body, and CTA around the goal: ${goal}`
        },
        {
          agentName: 'Video Agent',
          summary: `Queued a local preview package for ${this.readablePlatform(this.task.platform)} in ${this.readableFormat(this.task.format)} format.`
        }
      ]
    };

    this.readyVideoItems.unshift({
      topic,
      platform: this.readablePlatform(this.task.platform),
      format: this.readableFormat(this.task.format)
    });

    if (this.readyVideoItems.length > 6) {
      this.readyVideoItems.pop();
    }

    this.status = `Created a local preview at ${createdAt}. No backend task was submitted.`;
    this.running = false;
  }

  private readablePlatform(platform: VideoPlatform): string {
    switch (platform) {
      case 'youtube':
        return 'YouTube';
      case 'tiktok':
        return 'TikTok';
      case 'instagram':
        return 'Instagram';
      case 'linkedin':
        return 'LinkedIn';
      default:
        return platform;
    }
  }

  private readableFormat(format: VideoFormat): string {
    switch (format) {
      case 'short':
        return 'Short';
      case 'longform':
        return 'Longform';
      case 'carousel':
        return 'Carousel';
      default:
        return format;
    }
  }
}
