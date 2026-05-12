import { Component } from '@angular/core';
import { UserSummaryFacade } from './user-summary.facade';

@Component({
  selector: 'app-sign-in-page',
  standalone: true,
  templateUrl: './sign-in-page.component.html',
  styleUrl: './sign-in-page.component.css',
})
export class SignInPageComponent {
  constructor(readonly facade: UserSummaryFacade) {}
}
