import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/auth.service';
import { LoginRequest } from '../../core/models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  private readonly auth = inject(AuthService);

  readonly email = signal('');
  readonly password = signal('');
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  onEmail(value: string): void {
    this.email.set(value);
    this.errorMessage.set(null);
  }

  onPassword(value: string): void {
    this.password.set(value);
    this.errorMessage.set(null);
  }

  toggleShowPassword(): void {
    this.showPassword.update((v) => !v);
  }

  submit(): void {
    const email = this.email().trim();
    const password = this.password();

    if (!email || !password) {
      this.errorMessage.set('Please enter your email and password.');
      return;
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      this.errorMessage.set('Please enter a valid email address.');
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const req: LoginRequest = { email, password };
    this.auth.login(req).subscribe({
      next: (res) => {
        this.auth.setSession(res);
        this.auth.navigateAfterLogin();
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.password.set('');
        this.errorMessage.set(this.describeError(err));
      },
    });
  }

  private describeError(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 401 || err.status === 400) return 'Invalid email or password.';
      if (err.status === 0) return 'Cannot reach the server. Is the .NET API running on port 5050?';
      if (err.status >= 500) return `Server error (HTTP ${err.status}). Please try again.`;
      return `Login failed (HTTP ${err.status}).`;
    }
    return 'Login could not be completed. Please try again.';
  }
}
