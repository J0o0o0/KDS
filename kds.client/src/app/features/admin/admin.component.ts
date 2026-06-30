import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StationsService } from '../../core/services/stations.service';
import { AddOnsService } from '../../core/services/add-ons.service';
import { ComponentsService } from '../../core/services/components.service';
import { MenuItemsService } from '../../core/services/menu-items.service';
import { AuthService } from '../../core/services/auth.service';
import { UsersService } from '../../core/services/users.service';
import { TopBarComponent } from '../../shared/top-bar.component';
import { AddOn, ComponentAddOnLink, CreateMenuItemComponentRequest, CreateVariantRequest, ManagedUser, MenuComponent as MenuComponentModel, MenuItem, Role, Station } from '../../core/models';

type Tab = 'stations' | 'addons' | 'components' | 'menu-items' | 'users';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, TopBarComponent],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss'],
})
export class AdminComponent implements OnInit {
  readonly tabs: { id: Tab; label: string }[] = [
    { id: 'stations', label: 'Stations' },
    { id: 'addons', label: 'Add-ons' },
    { id: 'components', label: 'Components' },
    { id: 'menu-items', label: 'Menu items' },
    { id: 'users', label: 'Users' },
  ];
  readonly activeTab = signal<Tab>('stations');

  setActiveTab(tab: Tab): void {
    // Clear any in-progress edit so switching tabs doesn't leave a stale
    // half-filled edit form active in the background.
    this.cancelEditComponent();
    this.cancelEditMenuItem();
    this.activeTab.set(tab);
  }

  readonly stations = signal<Station[]>([]);
  readonly addOns = signal<AddOn[]>([]);
  readonly components = signal<MenuComponentModel[]>([]);
  readonly menuItems = signal<MenuItem[]>([]);
  readonly users = signal<ManagedUser[]>([]);

  readonly allRoles: Role[] = ['Admin', 'Cashier', 'Cook', 'Expediter'];

  newStationName = '';
  newStationColor = '#3b82f6';

  newAddOnName = '';
  newAddOnPrice: number | null = null;
  newAddOnIsRemoval = false;

  newComponentName = '';
  newComponentDescription = '';
  newComponentStation: Station | null = null;
  newComponentIsActive = true;
  newComponentAddOns: ComponentAddOnLink[] = [];
  newComponentSwapIds = new Set<number>();
  newComponentVariants: CreateVariantRequest[] = [];
  readonly editingComponentId = signal<number | null>(null);

  newMenuItemName = '';
  newMenuItemDescription = '';
  newMenuItemCategory = '';
  newMenuItemPrice: number | null = null;
  newMenuItemPrepTime = 10;
  newMenuItemIsActive = true;
  newMenuItemComponents: CreateMenuItemComponentRequest[] = [];
  readonly editingMenuItemId = signal<number | null>(null);
  readonly MenuItemMessage = signal<string | null>(null);
  readonly MenuItemError = signal(false);

  newUserFullName = '';
  newUserEmail = '';
  newUserPassword = '';
  newUserRole: Role = 'Cashier';
  newUserStationId: number | null = null;
  readonly registerMessage = signal<string | null>(null);
  readonly registerError = signal(false);

  constructor(
    private stationsSvc: StationsService,
    private addOnsSvc: AddOnsService,
    private componentsSvc: ComponentsService,
    private menuItemsSvc: MenuItemsService,
    private authSvc: AuthService,
    private usersSvc: UsersService
  ) { }

  ngOnInit(): void {
    this.loadAll();
  }

  private loadAll(): void {
    this.stationsSvc.getAll().subscribe((s) => this.stations.set(s));
    this.addOnsSvc.getAll().subscribe((a) => this.addOns.set(a));
    this.componentsSvc.getAll().subscribe((c) => this.components.set(c));
    this.menuItemsSvc.getAll().subscribe((m) => this.menuItems.set(m));
    this.usersSvc.getAll().subscribe((u) => this.users.set(u));
  }

  createStation(): void {
    if (!this.newStationName.trim()) return;
    this.stationsSvc
      .create({ name: this.newStationName, color: this.newStationColor, sortOrder: this.stations().length, isActive: true })
      .subscribe(() => {
        this.newStationName = '';
        this.stationsSvc.getAll().subscribe((s) => this.stations.set(s));
      });
  }

  toggleStation(s: Station): void {
    this.stationsSvc.toggleActive(s.id).subscribe(() => this.stationsSvc.getAll().subscribe((all) => this.stations.set(all)));
  }

  createAddOn(): void {
    if (!this.newAddOnName.trim()) return;
    this.addOnsSvc
      .create({ name: this.newAddOnName, price: this.newAddOnPrice ?? 0, isRemoval: this.newAddOnIsRemoval, isActive: true })
      .subscribe(() => {
        this.newAddOnName = '';
        this.newAddOnPrice = null;
        this.newAddOnIsRemoval = false;
        this.addOnsSvc.getAll().subscribe((a) => this.addOns.set(a));
      });
  }

  toggleAddOn(a: AddOn): void {
    this.addOnsSvc.toggleActive(a.id).subscribe(() => this.addOnsSvc.getAll().subscribe((all) => this.addOns.set(all)));
  }

  saveComponent(): void {
    if (!this.newComponentName.trim() || !this.newComponentStation) return;

    // Drop any incomplete variant rows (no name typed yet) before sending.
    const variants = this.newComponentVariants.filter((v) => v.name.trim().length > 0);

    const payload = {
      name: this.newComponentName,
      description: this.newComponentDescription || null,
      defaultStationId: this.newComponentStation.id,
      defaultStationName: this.newComponentStation.name,
      isActive: this.newComponentIsActive,
      addOns: this.newComponentAddOns,
      swappableWithIds: Array.from(this.newComponentSwapIds),
      variants,
    };

    const onSaved = () => {
      this.resetComponentForm();
      this.componentsSvc.getAll().subscribe((c) => this.components.set(c));
    };

    const editingId = this.editingComponentId();
    if (editingId !== null) {
      this.componentsSvc.update(editingId, payload).subscribe(onSaved);
    } else {
      this.componentsSvc.create(payload).subscribe(onSaved);
    }
  }

  startEditComponent(c: MenuComponentModel): void {
    this.editingComponentId.set(c.id);
    this.newComponentName = c.name;
    this.newComponentDescription = c.description ?? '';
    this.newComponentStation = this.stations().find((s) => s.id === c.defaultStationId) ?? null;
    this.newComponentIsActive = c.isActive;
    this.newComponentAddOns = c.allowedAddOns.map((a) => ({ addOnId: a.addOnId, maxQuantity: a.maxQuantity }));
    this.newComponentSwapIds = new Set(c.swappableWith.map((s) => s.componentId));
    this.newComponentVariants = c.variants.map((v) => ({ name: v.name, priceDelta: v.priceDelta, isDefault: v.isDefault }));
  }

  cancelEditComponent(): void {
    this.resetComponentForm();
  }

  private resetComponentForm(): void {
    this.editingComponentId.set(null);
    this.newComponentName = '';
    this.newComponentDescription = '';
    this.newComponentStation = null;
    this.newComponentIsActive = true;
    this.newComponentAddOns = [];
    this.newComponentSwapIds.clear();
    this.newComponentVariants = [];
  }

  // ---- Allowed add-ons (chip toggle + per-add-on max quantity) ----

  isAddOnSelected(addOnId: number): boolean {
    return this.newComponentAddOns.some((a) => a.addOnId === addOnId);
  }

  addOnMaxQuantity(addOnId: number): number {
    return this.newComponentAddOns.find((a) => a.addOnId === addOnId)?.maxQuantity ?? 1;
  }

  toggleNewComponentAddOn(addOnId: number): void {
    const idx = this.newComponentAddOns.findIndex((a) => a.addOnId === addOnId);
    if (idx >= 0) {
      this.newComponentAddOns.splice(idx, 1);
    } else {
      this.newComponentAddOns.push({ addOnId, maxQuantity: 1 });
    }
  }

  incrementAddOnMax(addOnId: number): void {
    const link = this.newComponentAddOns.find((a) => a.addOnId === addOnId);
    if (link) link.maxQuantity = Math.min(10, link.maxQuantity + 1);
  }

  decrementAddOnMax(addOnId: number): void {
    const link = this.newComponentAddOns.find((a) => a.addOnId === addOnId);
    if (link) link.maxQuantity = Math.max(1, link.maxQuantity - 1);
  }

  // ---- Swappable-with (chip toggle against existing components) ----

  toggleNewComponentSwap(componentId: number): void {
    if (this.newComponentSwapIds.has(componentId)) this.newComponentSwapIds.delete(componentId);
    else this.newComponentSwapIds.add(componentId);
  }

  // ---- Variants (repeatable row editor) ----

  addVariantRow(): void {
    // First variant added is the default by convention; user can change via radio.
    const isDefault = this.newComponentVariants.length === 0;
    this.newComponentVariants.push({ name: '', priceDelta: 0, isDefault });
  }

  removeVariantRow(index: number): void {
    const wasDefault = this.newComponentVariants[index]?.isDefault;
    this.newComponentVariants.splice(index, 1);
    // If we removed the default row, promote the first remaining row.
    if (wasDefault && this.newComponentVariants.length > 0) {
      this.newComponentVariants[0].isDefault = true;
    }
  }

  setDefaultVariant(index: number): void {
    this.newComponentVariants.forEach((v, i) => (v.isDefault = i === index));
  }

  variantNames(c: MenuComponentModel): string {
    return c.variants.map((v) => v.name).join(', ');
  }

  // ---- Components included (chip toggle + per-component quantity) ----

  isMenuItemComponentSelected(componentId: number): boolean {
    return this.newMenuItemComponents.some((c) => c.componentId === componentId);
  }

  menuItemComponentQty(componentId: number): number {
    return this.newMenuItemComponents.find((c) => c.componentId === componentId)?.quantity ?? 1;
  }

  toggleMenuItemComponent(componentId: number): void {
    const idx = this.newMenuItemComponents.findIndex((c) => c.componentId === componentId);
    if (idx >= 0) {
      this.newMenuItemComponents.splice(idx, 1);
    } else {
      this.newMenuItemComponents.push({ componentId, quantity: 1 });
    }
  }

  incrementMenuItemComponentQty(componentId: number): void {
    const entry = this.newMenuItemComponents.find((c) => c.componentId === componentId);
    if (entry) entry.quantity = Math.min(20, entry.quantity + 1);
  }

  decrementMenuItemComponentQty(componentId: number): void {
    const entry = this.newMenuItemComponents.find((c) => c.componentId === componentId);
    if (entry) entry.quantity = Math.max(1, entry.quantity - 1);
  }

  saveMenuItem(): void {
    if (!this.newMenuItemName.trim() || !this.newMenuItemCategory.trim() || !this.newMenuItemPrice || !this.newMenuItemPrepTime || !this.newMenuItemComponents)
    {
      this.MenuItemError.set(true);
      this.MenuItemMessage.set('Fill all fields');
      return;
    }
    this.MenuItemError.set(false)
    this.MenuItemMessage.set('');


    const payload = {
      name: this.newMenuItemName,
      description: this.newMenuItemDescription || null,
      category: this.newMenuItemCategory,
      basePrice: this.newMenuItemPrice ?? 0,
      prepTimeMinutes: this.newMenuItemPrepTime,
      isActive: this.newMenuItemIsActive,
      components: this.newMenuItemComponents,
    };

    const onSaved = () => {
      this.resetMenuItemForm();
      this.menuItemsSvc.getAll().subscribe((m) => this.menuItems.set(m));
    };

    const editingId = this.editingMenuItemId();
    if (editingId !== null) {
      this.menuItemsSvc.update(editingId, payload).subscribe(onSaved);
    } else {
      this.menuItemsSvc.create(payload).subscribe(onSaved);
    }
  }

  startEditMenuItem(m: MenuItem): void {
    this.editingMenuItemId.set(m.id);
    this.newMenuItemName = m.name;
    this.newMenuItemDescription = m.description ?? '';
    this.newMenuItemCategory = m.category;
    this.newMenuItemPrice = m.basePrice;
    this.newMenuItemPrepTime = m.prepTimeMinutes;
    this.newMenuItemIsActive = m.isActive;
    this.newMenuItemComponents = m.components.map((c) => ({ componentId: c.componentId, quantity: c.quantity }));
  }

  cancelEditMenuItem(): void {
    this.resetMenuItemForm();
  }

  private resetMenuItemForm(): void {
    this.editingMenuItemId.set(null);
    this.newMenuItemName = '';
    this.newMenuItemDescription = '';
    this.newMenuItemCategory = '';
    this.newMenuItemPrice = null;
    this.newMenuItemIsActive = true;
    this.newMenuItemComponents = [];
  }

  toggleMenuItem(m: MenuItem): void {
    this.menuItemsSvc.toggleActive(m.id).subscribe(() => this.menuItemsSvc.getAll().subscribe((all) => this.menuItems.set(all)));
  }

  registerUser(): void {
    this.registerMessage.set(null);
    if (!this.newUserFullName || !this.newUserEmail || !this.newUserPassword)
    {
      this.registerError.set(true);
      this.registerMessage.set('Fill all fields');
      return;
    }

    this.authSvc
      .register({
        fullName: this.newUserFullName,
        email: this.newUserEmail,
        password: this.newUserPassword,
        roles: [this.newUserRole],
        stationId: this.newUserRole === 'Cook' ? this.newUserStationId : null,
      })
      .subscribe({
        next: () => {
          this.registerError.set(false);
          this.registerMessage.set(`User ${this.newUserEmail} created.`);
          this.newUserFullName = '';
          this.newUserEmail = '';
          this.newUserPassword = '';
          this.newUserStationId = null;
          this.usersSvc.getAll().subscribe((u) => this.users.set(u));
        },
        error: (err) => {
          this.registerError.set(true);
          this.registerMessage.set(err.error ?? 'Could not create user.');
        },
      });
  }

  // ---- Manage existing users: toggle active, roles, station ----

  private refreshUsers(): void {
    this.usersSvc.getAll().subscribe((u) => this.users.set(u));
  }

  toggleUserActive(user: ManagedUser): void {
    this.usersSvc.toggleActive(user.id).subscribe(() => this.refreshUsers());
  }

  userHasRole(user: ManagedUser, role: Role): boolean {
    return user.roles.includes(role);
  }

  toggleUserRole(user: ManagedUser, role: Role): void {
    const obs = this.userHasRole(user, role)
      ? this.usersSvc.removeRole(user.id, role)
      : this.usersSvc.addRole(user.id, role);

    obs.subscribe({
      next: (updated) => {
        this.users.update((all) => all.map((u) => (u.id === updated.id ? updated : u)));
      },
      error: () => this.refreshUsers(),
    });
  }

  assignUserStation(user: ManagedUser, stationId: number | null): void {
    this.usersSvc.assignStation(user.id, stationId).subscribe((updated) => {
      this.users.update((all) => all.map((u) => (u.id === updated.id ? updated : u)));
    });
  }
}
