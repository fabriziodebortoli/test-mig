import { SettingsService } from './../../../../core/services/settings.service';


import { EasyStudioContextComponent } from './../../../../shared/components/easystudio-context/easystudio-context.component';
import { Component, ViewEncapsulation, Inject, forwardRef } from '@angular/core';

import { InfoService } from './../../../../core/services/info.service';

@Component({
  selector: 'tb-topbar-menu',
  templateUrl: './topbar-menu.component.html',
  styleUrls: ['./topbar-menu.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TopbarMenuComponent {

  isDesktop: boolean;
  isESActivated: boolean;

  constructor(
    public infoService: InfoService, 
    public settingsService : SettingsService ) {
      this.isDesktop = infoService.isDesktop;
      this.isESActivated = settingsService.IsEasyStudioActivated;
  }
}
