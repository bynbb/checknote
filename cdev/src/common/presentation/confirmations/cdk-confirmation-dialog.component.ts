import { Component, Inject } from '@angular/core';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { ConfirmationRequest } from '@cdev/common/application';

@Component({
  selector: 'app-cdk-confirmation-dialog',
  standalone: true,
  templateUrl: './cdk-confirmation-dialog.component.html',
  styleUrl: './cdk-confirmation-dialog.component.css',
})
export class CdkConfirmationDialogComponent {
  constructor(
    @Inject(DIALOG_DATA) readonly data: ConfirmationRequest,
    private readonly dialogRef: DialogRef<boolean, CdkConfirmationDialogComponent>,
  ) {}

  close(result: boolean): void {
    this.dialogRef.close(result);
  }
}
