import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

type TodoFilter = 'all' | 'active' | 'completed';

interface Todo {
  id: number;
  title: string;
  completed: boolean;
}

const STORAGE_KEY = 'bazel-angular-todos';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  protected readonly newTodo = signal('');
  protected readonly filter = signal<TodoFilter>('all');
  protected readonly todos = signal<Todo[]>(this.loadTodos());

  protected readonly activeCount = computed(() => this.todos().filter((todo) => !todo.completed).length);
  protected readonly completedCount = computed(() => this.todos().length - this.activeCount());
  protected readonly visibleTodos = computed(() => {
    const todos = this.todos();

    switch (this.filter()) {
      case 'active':
        return todos.filter((todo) => !todo.completed);
      case 'completed':
        return todos.filter((todo) => todo.completed);
      default:
        return todos;
    }
  });

  protected addTodo(): void {
    const title = this.newTodo().trim();

    if (!title) {
      return;
    }

    this.updateTodos([
      {
        id: Date.now(),
        title,
        completed: false,
      },
      ...this.todos(),
    ]);
    this.newTodo.set('');
  }

  protected toggleTodo(todoId: number): void {
    this.updateTodos(
      this.todos().map((todo) =>
        todo.id === todoId
          ? {
              ...todo,
              completed: !todo.completed,
            }
          : todo,
      ),
    );
  }

  protected deleteTodo(todoId: number): void {
    this.updateTodos(this.todos().filter((todo) => todo.id !== todoId));
  }

  protected clearCompleted(): void {
    this.updateTodos(this.todos().filter((todo) => !todo.completed));
  }

  protected setFilter(filter: TodoFilter): void {
    this.filter.set(filter);
  }

  private updateTodos(todos: Todo[]): void {
    this.todos.set(todos);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(todos));
  }

  private loadTodos(): Todo[] {
    const storedTodos = localStorage.getItem(STORAGE_KEY);

    if (!storedTodos) {
      return [
        {
          id: 1,
          title: 'Build the app with Bazel',
          completed: false,
        },
      ];
    }

    try {
      return JSON.parse(storedTodos) as Todo[];
    } catch {
      return [];
    }
  }
}
