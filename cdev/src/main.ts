import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from '@cdev/app/app.component';
import { provideCommonModule } from '@cdev/common/composition';
import { provideTodosModule } from '@cdev/modules/todos/composition';
import { provideUsersModule } from '@cdev/modules/users/composition';

bootstrapApplication(AppComponent, {
  providers: [provideCommonModule(), provideUsersModule(), provideTodosModule()],
}).catch((error) => console.error(error));
