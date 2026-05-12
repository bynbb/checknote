import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { GlobalErrorBannerComponent } from '@cdev/common/presentation';
import { UserSummaryComponent } from '@cdev/modules/users/presentation';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [GlobalErrorBannerComponent, UserSummaryComponent, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {}
