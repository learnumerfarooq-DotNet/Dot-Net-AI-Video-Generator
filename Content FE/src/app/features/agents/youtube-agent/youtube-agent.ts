import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentsStore } from '../store/agents.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-youtube-agent',
  imports: [CommonModule, ReactiveFormsModule, AgentChatComponent],
  templateUrl: './youtube-agent.html',
  styleUrl: './youtube-agent.css'
})
export class YoutubeAgentComponent implements OnInit {
  protected readonly agentsStore = inject(AgentsStore);
  protected readonly rootStore = inject(ContentFactoryStore);
  apiForm!: FormGroup;
  saved = false;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.apiForm = this.fb.group({
      clientId: ['', Validators.required],
      clientSecret: ['', Validators.required],
      refreshToken: ['']
    });
  }

  saveConfig(): void {
    if (this.apiForm.valid) {
      this.saved = true;
      setTimeout(() => this.saved = false, 3000);
    }
  }
}
