import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { AgentChatComponent } from '../../../shared/agent-chat/agent-chat';

@Component({
  selector: 'app-main-brain',
  imports: [CommonModule, ReactiveFormsModule, AgentChatComponent],
  templateUrl: './main-brain.html',
  styleUrl: './main-brain.css'
})
export class MainBrainComponent {
  protected readonly store = inject(ContentFactoryStore);
}
