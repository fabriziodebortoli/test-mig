import { Subscription } from 'rxjs/Subscription';
import { Component, ViewEncapsulation, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { TbComponent } from './../../../../shared/components/tb.component';


import { InfoService } from './../../../../core/services/info.service';
import { SettingsService } from './../../../../core/services/settings.service';
import { HttpService } from './../../../../core/services/http.service';

import { EasyStudioContextComponent } from './../../../../shared/components/easystudio-context/easystudio-context.component';
import { MenuService } from './../../../../menu/services/menu.service';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';

@Component({
  selector: 'tb-topbar-menu',
  templateUrl: './topbar-menu.component.html',
  styleUrls: ['./topbar-menu.component.scss']
})
export class TopbarMenuComponent extends TbComponent implements OnDestroy{

  isDesktop: boolean;
  isESActivated: boolean;
  isBPMActivated: boolean;
  subscription: Subscription

  constructor(
    public infoService: InfoService,
    public settingsService: SettingsService,
    public httpService: HttpService,
    public menuService: MenuService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();

    this.isDesktop = infoService.isDesktop;
    this.isESActivated = settingsService.IsEasyStudioActivated;
    this.subscription= httpService.isActivated('BPM','Connector').subscribe( res => {
      this.isBPMActivated = res.result;
    });
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }
  
  clearCachedData() {
    this.menuService.invalidateCache();
}
}
