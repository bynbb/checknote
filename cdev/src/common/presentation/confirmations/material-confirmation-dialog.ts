import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationDialog, ConfirmationRequest } from '@cdev/common/application';
import { firstValueFrom } from 'rxjs';
import { MaterialConfirmationDialogComponent } from './material-confirmation-dialog.component';

@Injectable()
export class MaterialConfirmationDialog implements ConfirmationDialog {
  constructor(private readonly dialog: MatDialog) {}

  async confirm(request: ConfirmationRequest): Promise<boolean> {
    const dialogRef = this.dialog.open<MaterialConfirmationDialogComponent, ConfirmationRequest, boolean>(
      MaterialConfirmationDialogComponent,
      {
        autoFocus: 'dialog',
        data: request,
        maxWidth: '92vw',
        restoreFocus: true,
        width: '420px',
      },
    );

    return Boolean(await firstValueFrom(dialogRef.afterClosed()));
  }
}
