import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogTitle,
} from '@angular/material/dialog';
import { ConfirmationRequest } from '@cdev/common/application';

@Component({
  selector: 'app-material-confirmation-dialog',
  standalone: true,
  imports: [MatButtonModule, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogTitle],
  templateUrl: './material-confirmation-dialog.component.html',
  styleUrl: './material-confirmation-dialog.component.css',
})
export class MaterialConfirmationDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) readonly data: ConfirmationRequest) {}
}
