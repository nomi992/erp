import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectButtonModule } from 'primeng/selectbutton';
import { TooltipModule } from 'primeng/tooltip';
import { ThemeService } from '../../../core/theme/theme.service';
import { PrimaryColorName, ThemeName } from '../../../core/theme/theme.models';

@Component({
  selector: 'app-theme-settings',
  imports: [FormsModule, CardModule, ButtonModule, SelectButtonModule, TooltipModule],
  templateUrl: './theme-settings.html',
  styleUrl: './theme-settings.scss',
})
export class ThemeSettings {
  readonly themeService = inject(ThemeService);

  onPresetChange(name: ThemeName): void {
    this.themeService.setPreset(name);
  }

  onModeChange(mode: 'light' | 'dark'): void {
    this.themeService.setMode(mode);
  }

  onPrimaryColorChange(name: PrimaryColorName): void {
    this.themeService.setPrimaryColor(name);
  }
}
