
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

  constructor(private infoService: InfoService) {
    this.isDesktop = infoService.isDesktop;
  }
}
