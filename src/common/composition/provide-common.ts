import { makeEnvironmentProviders } from '@angular/core';
import { BrowserClock } from '../infrastructure/browser-clock';
import { MockEndpointLogger } from '../infrastructure/mock-endpoint-logger';

export function provideCommonModule() {
  return makeEnvironmentProviders([BrowserClock, MockEndpointLogger]);
}
