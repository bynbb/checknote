import { Component } from '@angular/core';
import { TodosPageComponent } from '@cdev/modules/todos/presentation';
import { UserSummaryComponent } from '@cdev/modules/users/presentation';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [UserSummaryComponent, TodosPageComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {}
