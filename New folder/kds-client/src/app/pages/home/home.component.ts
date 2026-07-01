import { Component, computed, inject } from '@angular/core';
import { AuthService } from '../../core/auth.service';
import { ROLES } from '../../core/models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {
  private readonly auth = inject(AuthService);

  readonly fullName = computed(() => this.auth.user()?.fullName ?? 'User');
  readonly email = computed(() => this.auth.user()?.email ?? '');

  readonly isAdmin = computed(() => this.auth.hasRole(ROLES.ADMIN));
  readonly isCashier = computed(() => this.auth.hasRole(ROLES.CASHIER));
  readonly isCook = computed(() => this.auth.hasRole(ROLES.COOK));

  readonly roleLabel = computed(() => {
    if (this.isAdmin()) return 'Administrator';
    if (this.isCashier()) return 'Cashier';
    if (this.isCook()) return 'Cook';
    return 'Team Member';
  });

  logout(): void {
    this.auth.logout();
  }
}
