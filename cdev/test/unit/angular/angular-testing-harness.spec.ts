import { HttpClient, HttpHandlerFn, HttpInterceptorFn, HttpRequest, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component, InjectionToken, inject } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { CanActivateFn, provideRouter, Router } from '@angular/router';
import { describe, expect, it } from 'vitest';

const AUTH_STATE = new InjectionToken<{ readonly authenticated: boolean }>('auth state');
const API_TOKEN = new InjectionToken<string>('api token');

@Component({
  selector: 'cdev-angular-harness-probe',
  standalone: true,
  template: '<span class="probe">{{ label }}</span>',
})
class AngularHarnessProbeComponent {
  readonly label = inject(API_TOKEN);
}

const bearerInterceptor: HttpInterceptorFn = (
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
) => next(request.clone({ setHeaders: { Authorization: `Bearer ${inject(API_TOKEN)}` } }));

const authGuard: CanActivateFn = () => inject(AUTH_STATE).authenticated;

describe('Angular testing harness', () => {
  it('creates standalone component fixtures with dependency injection', async () => {
    await TestBed.configureTestingModule({
      imports: [AngularHarnessProbeComponent],
      providers: [{ provide: API_TOKEN, useValue: 'test-token' }],
    }).compileComponents();

    const fixture = TestBed.createComponent(AngularHarnessProbeComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.probe')?.textContent).toBe('test-token');
  });

  it('supports router guard tests through TestBed injection context', () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AUTH_STATE, useValue: { authenticated: true } },
      ],
    });

    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));

    expect(result).toBe(true);
    expect(TestBed.inject(Router)).toBeTruthy();
  });

  it('supports HTTP interceptor tests without a live backend', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([bearerInterceptor])),
        provideHttpClientTesting(),
        { provide: API_TOKEN, useValue: 'abc123' },
      ],
    });

    TestBed.inject(HttpClient).get('/api/current-user').subscribe();

    const request = TestBed.inject(HttpTestingController).expectOne('/api/current-user');
    expect(request.request.headers.get('Authorization')).toBe('Bearer abc123');

    request.flush({ ok: true });
    TestBed.inject(HttpTestingController).verify();
  });
});
