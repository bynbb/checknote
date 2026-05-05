import { Clock } from '@cdev/common/application';

export class BrowserClock implements Clock {
  now(): Date {
    return new Date();
  }
}
