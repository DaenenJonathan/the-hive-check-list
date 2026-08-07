import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ConfirmDialogOptions {
  title: string;
  message: string;
  confirmLabel: string;
  cancelLabel?: string;
  variant?: 'danger' | 'success' | 'primary';
}

interface ConfirmDialogState extends ConfirmDialogOptions {
  resolve: (result: boolean) => void;
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly stateSubject = new BehaviorSubject<ConfirmDialogState | null>(null);
  readonly state$ = this.stateSubject.asObservable();

  confirm(options: ConfirmDialogOptions): Promise<boolean> {
    return new Promise<boolean>(resolve => {
      this.stateSubject.next({ ...options, resolve });
    });
  }

  respond(result: boolean): void {
    const current = this.stateSubject.value;
    if (!current) return;
    this.stateSubject.next(null);
    current.resolve(result);
  }
}
