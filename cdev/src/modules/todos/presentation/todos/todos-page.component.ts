import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TodosPageFacade } from './todos-page.facade';

@Component({
  selector: 'app-todos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './todos-page.component.html',
  styleUrl: './todos-page.component.css',
})
export class TodosPageComponent {
  constructor(readonly facade: TodosPageFacade) {}
}
