import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideCommonModule } from './common/composition/provide-common';
import { provideTodosModule } from './modules/todos/composition';
import { provideUsersModule } from './modules/users/composition';

bootstrapApplication(AppComponent, {
  providers: [provideCommonModule(), provideUsersModule(), provideTodosModule()],
}).catch((error) => console.error(error));
