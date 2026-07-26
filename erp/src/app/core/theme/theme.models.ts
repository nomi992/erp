export type ThemeName = 'aura' | 'lara' | 'nora';
export type ThemeMode = 'light' | 'dark';
export type PrimaryColorName =
  | 'emerald'
  | 'blue'
  | 'indigo'
  | 'violet'
  | 'purple'
  | 'rose'
  | 'orange'
  | 'cyan';

export interface ThemeOption {
  name: ThemeName;
  label: string;
}

export interface PrimaryColorOption {
  name: PrimaryColorName;
  label: string;
  swatch: string;
}

export const THEME_OPTIONS: ThemeOption[] = [
  { name: 'aura', label: 'Aura' },
  { name: 'lara', label: 'Lara' },
  { name: 'nora', label: 'Nora' },
];

export const PRIMARY_COLOR_OPTIONS: PrimaryColorOption[] = [
  { name: 'emerald', label: 'Emerald', swatch: '#10b981' },
  { name: 'blue', label: 'Blue', swatch: '#3b82f6' },
  { name: 'indigo', label: 'Indigo', swatch: '#6366f1' },
  { name: 'violet', label: 'Violet', swatch: '#8b5cf6' },
  { name: 'purple', label: 'Purple', swatch: '#a855f7' },
  { name: 'rose', label: 'Rose', swatch: '#f43f5e' },
  { name: 'orange', label: 'Orange', swatch: '#f97316' },
  { name: 'cyan', label: 'Cyan', swatch: '#06b6d4' },
];

export const DARK_MODE_SELECTOR = 'app-dark';
