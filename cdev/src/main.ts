import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { AppComponent } from '@cdev/app/app.component';
import { routes } from '@cdev/app/app.routes';
import { provideCommonModule } from '@cdev/common/composition';
import { provideTodosModule } from '@cdev/modules/todos/composition';
import { provideUsersModule } from '@cdev/modules/users/composition';

bootstrapApplication(AppComponent, {
  providers: [provideRouter(routes), provideCommonModule(), provideUsersModule(), provideTodosModule()],
}).catch((error) => console.error(error));
