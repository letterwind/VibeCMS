import { Injectable, ComponentRef, ApplicationRef, createComponent, EnvironmentInjector } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ConfirmDialogComponent, ConfirmDialogOptions } from './confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmDialogService {
  private dialogRef: ComponentRef<ConfirmDialogComponent> | null = null;

  constructor(
    private appRef: ApplicationRef,
    private injector: EnvironmentInjector
  ) {}

  confirm(options: ConfirmDialogOptions): Observable<boolean> {
    const result = new Subject<boolean>();

    // Create component
    this.dialogRef = createComponent(ConfirmDialogComponent, {
      environmentInjector: this.injector
    });

    // Set inputs
    this.dialogRef.instance.title = options.title;
    this.dialogRef.instance.message = options.message;
    this.dialogRef.instance.confirmText = options.confirmText || '確定';
    this.dialogRef.instance.cancelText = options.cancelText || '取消';
    this.dialogRef.instance.type = options.type || 'warning';
    this.dialogRef.instance.isOpen = true;

    // Subscribe to outputs
    this.dialogRef.instance.onConfirm.subscribe(() => {
      result.next(true);
      result.complete();
      this.destroyDialog();
    });

    this.dialogRef.instance.onCancel.subscribe(() => {
      result.next(false);
      result.complete();
      this.destroyDialog();
    });

    // Attach to DOM
    document.body.appendChild(this.dialogRef.location.nativeElement);
    this.appRef.attachView(this.dialogRef.hostView);

    return result.asObservable();
  }

  private destroyDialog(): void {
    if (this.dialogRef) {
      this.appRef.detachView(this.dialogRef.hostView);
      this.dialogRef.destroy();
      this.dialogRef = null;
    }
  }
}
