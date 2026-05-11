import { LiveAnnouncer } from '@angular/cdk/a11y';
import { Component, effect, inject } from '@angular/core';
import { GlobalErrorState } from './global-error-state';

@Component({
  selector: 'cdev-global-error-banner',
  standalone: true,
  templateUrl: './global-error-banner.component.html',
  styleUrl: './global-error-banner.component.css',
})
export class GlobalErrorBannerComponent {
  readonly errors = inject(GlobalErrorState);
  private readonly liveAnnouncer = inject(LiveAnnouncer);

  constructor() {
    effect(() => {
      const error = this.errors.current();

      if (error) {
        void this.liveAnnouncer.announce(`${error.code}. ${error.message}`, 'assertive');
      } else {
        this.liveAnnouncer.clear();
      }
    });
  }
}
