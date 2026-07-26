import { DOCUMENT } from '@angular/common';
import { Injectable, computed, inject, signal } from '@angular/core';
import { PrimeNG } from 'primeng/config';
import { updatePrimaryPalette } from '@primeuix/themes';
import Aura from '@primeng/themes/aura';
import Lara from '@primeng/themes/lara';
import Nora from '@primeng/themes/nora';
import {
  DARK_MODE_SELECTOR,
  PRIMARY_COLOR_OPTIONS,
  PrimaryColorName,
  THEME_OPTIONS,
  ThemeMode,
  ThemeName,
} from './theme.models';

const PRESET_STORAGE_KEY = 'erp.theme.preset';
const MODE_STORAGE_KEY = 'erp.theme.mode';
const PRIMARY_STORAGE_KEY = 'erp.theme.primary';

const PALETTE_SHADES = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950] as const;

const PRESETS: Record<ThemeName, unknown> = {
  aura: Aura,
  lara: Lara,
  nora: Nora,
};

const PRIMARY_COLOR_NAMES = new Set(PRIMARY_COLOR_OPTIONS.map((option) => option.name));

function readStoredPreset(): ThemeName {
  const stored = localStorage.getItem(PRESET_STORAGE_KEY) as ThemeName | null;
  return stored && stored in PRESETS ? stored : 'aura';
}

function readStoredMode(): ThemeMode {
  const stored = localStorage.getItem(MODE_STORAGE_KEY) as ThemeMode | null;
  if (stored === 'light' || stored === 'dark') {
    return stored;
  }
  return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function readStoredPrimaryColor(): PrimaryColorName {
  const stored = localStorage.getItem(PRIMARY_STORAGE_KEY) as PrimaryColorName | null;
  return stored && PRIMARY_COLOR_NAMES.has(stored) ? stored : 'emerald';
}

function buildPrimaryPalette(name: PrimaryColorName): Record<(typeof PALETTE_SHADES)[number], string> {
  return Object.fromEntries(PALETTE_SHADES.map((shade) => [shade, `{${name}.${shade}}`])) as Record<
    (typeof PALETTE_SHADES)[number],
    string
  >;
}

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly primeng = inject(PrimeNG);
  private readonly document = inject(DOCUMENT);

  private readonly preset = signal<ThemeName>(readStoredPreset());
  private readonly mode = signal<ThemeMode>(readStoredMode());
  private readonly primaryColor = signal<PrimaryColorName>(readStoredPrimaryColor());

  readonly currentPreset = computed(() => this.preset());
  readonly currentMode = computed(() => this.mode());
  readonly currentPrimaryColor = computed(() => this.primaryColor());
  readonly presetOptions = THEME_OPTIONS;
  readonly primaryColorOptions = PRIMARY_COLOR_OPTIONS;

  constructor() {
    this.applyPreset(this.preset());
    this.applyMode(this.mode());
    this.applyPrimaryColor(this.primaryColor());
  }

  setPreset(name: ThemeName): void {
    if (name === this.preset()) {
      return;
    }
    localStorage.setItem(PRESET_STORAGE_KEY, name);
    this.preset.set(name);
    this.applyPreset(name);
    this.applyPrimaryColor(this.primaryColor());
  }

  setMode(mode: ThemeMode): void {
    if (mode === this.mode()) {
      return;
    }
    localStorage.setItem(MODE_STORAGE_KEY, mode);
    this.mode.set(mode);
    this.applyMode(mode);
  }

  toggleMode(): void {
    this.setMode(this.mode() === 'dark' ? 'light' : 'dark');
  }

  setPrimaryColor(name: PrimaryColorName): void {
    if (name === this.primaryColor()) {
      return;
    }
    localStorage.setItem(PRIMARY_STORAGE_KEY, name);
    this.primaryColor.set(name);
    this.applyPrimaryColor(name);
  }

  private applyPreset(name: ThemeName): void {
    this.primeng.theme.set({
      preset: PRESETS[name],
      options: { darkModeSelector: `.${DARK_MODE_SELECTOR}` },
    });
  }

  private applyMode(mode: ThemeMode): void {
    this.document.documentElement.classList.toggle(DARK_MODE_SELECTOR, mode === 'dark');
  }

  private applyPrimaryColor(name: PrimaryColorName): void {
    updatePrimaryPalette(buildPrimaryPalette(name));
  }
}
