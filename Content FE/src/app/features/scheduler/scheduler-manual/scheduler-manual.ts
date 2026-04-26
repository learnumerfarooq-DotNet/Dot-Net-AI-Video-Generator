import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { SchedulerStore } from '../store/scheduler.store';

@Component({
  selector: 'app-scheduler-manual',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './scheduler-manual.html',
  styleUrl: './scheduler-manual.css'
})
export class SchedulerManualComponent implements OnInit {
  protected readonly schedulerStore = inject(SchedulerStore);
  protected readonly rootStore = inject(ContentFactoryStore);
  scheduleForm!: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    const draft = this.schedulerStore.manualSchedule();
    this.scheduleForm = this.fb.group({
      name: [draft.name || '', Validators.required],
      agentKey: [draft.agentKey || 'main-brain', Validators.required],
      trigger: [draft.trigger || '0 0 * * *', Validators.required],
      notes: [draft.notes || ''],
      isEnabled: [draft.isEnabled ?? true]
    });
  }

  submit(): void {
    if (!this.scheduleForm.valid) return;
    const val = this.scheduleForm.value;
    (Object.keys(val) as Array<keyof typeof val>).forEach((key) => {
      this.schedulerStore.updateManualScheduleField(key as any, val[key]);
    });
    void this.schedulerStore.createManualSchedule(() => this.rootStore.refreshAll());
  }
}
