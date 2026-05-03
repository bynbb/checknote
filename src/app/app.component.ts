import { Component } from '@angular/core';
import { TodosPageComponent } from '../modules/todos/presentation/todos-page.component';
import { UserSummaryComponent } from '../modules/users/presentation/user-summary.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [UserSummaryComponent, TodosPageComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {}
