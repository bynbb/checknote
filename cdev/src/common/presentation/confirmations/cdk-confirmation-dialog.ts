import { Dialog } from '@angular/cdk/dialog';
import { Injectable } from '@angular/core';
import { ConfirmationDialog, ConfirmationRequest } from '@cdev/common/application';
import { firstValueFrom } from 'rxjs';
import { CdkConfirmationDialogComponent } from './cdk-confirmation-dialog.component';

@Injectable()
export class CdkConfirmationDialog implements ConfirmationDialog {
  constructor(private readonly dialog: Dialog) {}

  async confirm(request: ConfirmationRequest): Promise<boolean> {
    const dialogRef = this.dialog.open<boolean, ConfirmationRequest, CdkConfirmationDialogComponent>(
      CdkConfirmationDialogComponent,
      {
        ariaLabel: request.title,
        ariaModal: true,
        autoFocus: 'dialog',
        backdropClass: 'cdk-overlay-dark-backdrop',
        data: request,
        maxWidth: '92vw',
        panelClass: 'checknote-confirmation-dialog-pane',
        restoreFocus: true,
        role: request.tone === 'danger' ? 'alertdialog' : 'dialog',
        width: '420px',
      },
    );

    return Boolean(await firstValueFrom(dialogRef.closed));
  }
}
