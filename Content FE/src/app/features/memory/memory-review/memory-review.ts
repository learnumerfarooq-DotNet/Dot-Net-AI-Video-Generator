import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { MemoryStore } from '../store/memory.store';
import { MemoryRecord } from '../../../core/models/content-factory.models';

@Component({
  selector: 'app-memory-review',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './memory-review.html',
  styleUrl: './memory-review.css'
})
export class MemoryReviewComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  protected readonly memoryStore = inject(MemoryStore);
  reviewForms: Record<string, FormGroup> = {};

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    const queue = this.memoryStore.reviewQueue();
    if (queue) {
      queue.forEach((item: any) => {
        this.reviewForms[item.id] = this.fb.group({
          title: [item.title, Validators.required],
          content: [item.content, Validators.required]
        });
      });
    }
  }

  getForm(item: MemoryRecord): FormGroup {
    if (!this.reviewForms[item.id]) {
      this.reviewForms[item.id] = this.fb.group({
        title: [item.title, Validators.required],
        content: [item.content, Validators.required]
      });
    }
    return this.reviewForms[item.id];
  }

  approve(item: MemoryRecord): void {
    const form = this.reviewForms[item.id];
    if (form) {
      this.memoryStore.updateMemoryDraft(item, 'title', form.value.title);
      this.memoryStore.updateMemoryDraft(item, 'content', form.value.content);
    }
    void this.memoryStore.approveMemory(item, () => this.store.refreshAll());
  }

  reject(item: MemoryRecord): void {
    void this.memoryStore.rejectMemory(item, () => this.store.refreshAll());
  }
}
