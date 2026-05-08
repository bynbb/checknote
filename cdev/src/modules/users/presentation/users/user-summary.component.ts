import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { UserSummaryFacade } from './user-summary.facade';

@Component({
  selector: 'app-user-summary',
  standalone: true,
  imports: [MatCardModule],
  templateUrl: './user-summary.component.html',
  styleUrl: './user-summary.component.css',
})
export class UserSummaryComponent {
  constructor(readonly facade: UserSummaryFacade) {}
}
