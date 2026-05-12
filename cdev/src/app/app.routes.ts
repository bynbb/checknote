import { Routes } from '@angular/router';
import { authenticatedRouteGuard, signInRouteGuard } from '@cdev/common/composition/auth/auth-route.guard';
import { TodosPageComponent } from '@cdev/modules/todos/presentation';
import { SignInPageComponent } from '@cdev/modules/users/presentation';

export const routes: Routes = [
  {
    path: '',
    component: TodosPageComponent,
    canActivate: [authenticatedRouteGuard],
  },
  {
    path: 'sign-in',
    component: SignInPageComponent,
    canActivate: [signInRouteGuard],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
