import { Component } from '@angular/core';
import { UserSummaryFacade } from './user-summary.facade';

@Component({
  selector: 'app-user-summary',
  standalone: true,
  templateUrl: './user-summary.component.html',
  styleUrl: './user-summary.component.css',
})
export class UserSummaryComponent {
  constructor(readonly facade: UserSummaryFacade) {}
}
