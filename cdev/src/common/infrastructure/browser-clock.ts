import { Clock } from '../application/ports/clock';

export class BrowserClock implements Clock {
  now(): Date {
    return new Date();
  }
}
