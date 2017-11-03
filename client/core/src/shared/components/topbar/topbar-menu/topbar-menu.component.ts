import { Component, ViewEncapsulation } from '@angular/core';

import { InfoService } from './../../../../core/services/info.service';
import { SettingsService } from './../../../../core/services/settings.service';

import { EasyStudioContextComponent } from './../../../../shared/components/easystudio-context/easystudio-context.component';

@Component({
  selector: 'tb-topbar-menu',
  templateUrl: './topbar-menu.component.html',
  styleUrls: ['./topbar-menu.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TopbarMenuComponent {

  isDesktop: boolean;
  isESActivated: boolean;
  isBPMActivated: boolean;

  constructor(
    public infoService: InfoService,
    public settingsService: SettingsService) {
    this.isDesktop = infoService.isDesktop;
    this.isESActivated = settingsService.IsEasyStudioActivated;
    this.isBPMActivated = infoService.isBPMActivated;
  }
}
