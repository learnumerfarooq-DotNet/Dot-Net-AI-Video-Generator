import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { MemoryRecord } from '../../../core/models/content-factory.models';

@Component({
  selector: 'app-memory-review',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './memory-review.html',
  styleUrl: './memory-review.css'
})
export class MemoryReviewComponent implements OnInit {
  protected readonly store = inject(ContentFactoryStore);
  reviewForms: Record<string, FormGroup> = {};

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    const memory = this.store.memory();
    if (memory) {
      memory.reviewQueue.forEach((item) => {
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
      this.store.updateMemoryDraft(item, 'title', form.value.title);
      this.store.updateMemoryDraft(item, 'content', form.value.content);
    }
    void this.store.approveMemory(item);
  }

  reject(item: MemoryRecord): void {
    void this.store.rejectMemory(item);
  }
}
