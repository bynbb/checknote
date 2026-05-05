import { makeEnvironmentProviders } from '@angular/core';
import { BrowserClock, MockEndpointLogger } from '@cdev/common/infrastructure';

export function provideCommonModule() {
  return makeEnvironmentProviders([BrowserClock, MockEndpointLogger]);
}
