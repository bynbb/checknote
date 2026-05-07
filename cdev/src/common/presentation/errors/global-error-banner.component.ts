import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { GlobalErrorState } from './global-error-state';

@Component({
  selector: 'cdev-global-error-banner',
  standalone: true,
  imports: [MatButtonModule, MatCardModule],
  templateUrl: './global-error-banner.component.html',
  styleUrl: './global-error-banner.component.css',
})
export class GlobalErrorBannerComponent {
  readonly errors = inject(GlobalErrorState);
}
