import { Component, ViewEncapsulation, Inject, forwardRef } from '@angular/core';

import { AppConfigService } from './../../../../core/services/app-config.service';

@Component({
  selector: 'tb-topbar-menu',
  templateUrl: './topbar-menu.component.html',
  styleUrls: ['./topbar-menu.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TopbarMenuComponent {

  isDesktop: boolean;

  constructor(private appConfigService: AppConfigService) {
    this.isDesktop = appConfigService.config.isDesktop;
  }
}
