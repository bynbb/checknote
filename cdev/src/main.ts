import { bootstrapApplication } from '@angular/platform-browser';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { AppComponent } from '@cdev/app/app.component';
import { provideCommonModule } from '@cdev/common/composition';
import { provideTodosModule } from '@cdev/modules/todos/composition';
import { provideUsersModule } from '@cdev/modules/users/composition';

bootstrapApplication(AppComponent, {
  providers: [provideAnimationsAsync(), provideCommonModule(), provideUsersModule(), provideTodosModule()],
}).catch((error) => console.error(error));
