import { Component, HostListener } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfirmDialogService, ConfirmDialogOptions } from '../../services/confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  standalone: false
})
export class ConfirmDialogComponent {
  state$: Observable<(ConfirmDialogOptions & { resolve: (result: boolean) => void }) | null>;

  constructor(public confirmDialogService: ConfirmDialogService) {
    this.state$ = this.confirmDialogService.state$;
  }

  @HostListener('window:keydown.escape')
  onEscape(): void {
    this.confirmDialogService.respond(false);
  }
}
