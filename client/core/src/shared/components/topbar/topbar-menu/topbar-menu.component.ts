import { EasyStudioContextComponent } from './../../../../shared/components/easystudio-context/easystudio-context.component';
import { UrlService } from './../../../../core/services/url.service';
import { Component, ViewEncapsulation, Inject, forwardRef } from '@angular/core';


@Component({
  selector: 'tb-topbar-menu',
  templateUrl: './topbar-menu.component.html',
  styleUrls: ['./topbar-menu.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TopbarMenuComponent{
  constructor(private urlService: UrlService) {
  }
}
