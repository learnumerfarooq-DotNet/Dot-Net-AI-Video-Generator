import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ContentFactoryStore } from '../../../core/store/content-factory.store';
import { SchedulerStore } from '../store/scheduler.store';
import { AgentsStore } from '../../agents/store/agents.store';

@Component({
  selector: 'app-scheduler-manual',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './scheduler-manual.html',
  styleUrl: './scheduler-manual.css'
})
export class SchedulerManualComponent implements OnInit {
  protected readonly schedulerStore = inject(SchedulerStore);
  protected readonly rootStore = inject(ContentFactoryStore);
  protected readonly agentsStore = inject(AgentsStore);
  scheduleForm!: FormGroup;

  triggerTypes = ['Once', 'Daily', 'Hourly', 'Custom Cron'];

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    const draft = this.schedulerStore.manualSchedule();
    this.scheduleForm = this.fb.group({
      name: [draft.name || '', Validators.required],
      agentKey: [draft.agentKey || 'main-brain', Validators.required],
      triggerType: ['Once', Validators.required],
      triggerValue: [draft.trigger || '', Validators.required],
      notes: [draft.notes || ''],
      isEnabled: [draft.isEnabled ?? true]
    });

    this.scheduleForm.get('triggerType')?.valueChanges.subscribe(type => {
      if (type === 'Once') {
        this.scheduleForm.get('triggerValue')?.setValue(new Date().toISOString().slice(0, 16));
      } else if (type === 'Daily') {
        this.scheduleForm.get('triggerValue')?.setValue('0 12 * * *');
      } else if (type === 'Hourly') {
        this.scheduleForm.get('triggerValue')?.setValue('0 * * * *');
      } else {
        this.scheduleForm.get('triggerValue')?.setValue('');
      }
    });
  }

  submit(): void {
    if (!this.scheduleForm.valid) return;
    const val = this.scheduleForm.value;
    
    // Convert form values to ManualScheduleDraft
    const draftUpdates = {
      name: val.name,
      agentKey: val.agentKey,
      trigger: val.triggerValue,
      notes: val.notes,
      isEnabled: val.isEnabled
    };

    (Object.keys(draftUpdates) as Array<keyof typeof draftUpdates>).forEach((key) => {
      this.schedulerStore.updateManualScheduleField(key as any, draftUpdates[key]);
    });
    void this.schedulerStore.createManualSchedule(() => this.rootStore.refreshAll());
  }

  toggleSchedule(id: string) {
    this.schedulerStore.toggleScheduleEnabled(id);
  }

  deleteSchedule(id: string) {
    if (confirm('Are you sure you want to delete this schedule?')) {
      this.schedulerStore.deleteSchedule(id);
    }
  }

  runNow(id: string) {
    this.schedulerStore.runScheduleNow(id);
  }
}
