import { Component, ViewEncapsulation, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { TbComponent, InfoService, SettingsService, HttpService, TbComponentService } from '@taskbuilder/core';

import { MenuService } from './../../../services/menu.service';

@Component({
  selector: 'tb-topbar-menu',
  templateUrl: './topbar-menu.component.html',
  styleUrls: ['./topbar-menu.component.scss']
})
export class TopbarMenuComponent extends TbComponent implements OnDestroy {

  isDesktop: boolean;
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
    this.subscription = httpService.isActivated('BPM', 'Connector').subscribe(res => {
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
