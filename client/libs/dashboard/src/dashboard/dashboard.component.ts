import { Component, ChangeDetectorRef, OnInit, OnDestroy } from '@angular/core';

import {
  HttpMenuService, SettingsService, UtilsService,
  ImageService, MenuService, TbComponent, TbComponentService
} from '@taskbuilder/core';

@Component({
  selector: 'tb-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent extends TbComponent implements OnInit, OnDestroy {

  favorites: Array<any> = [];
  private subscriptions = [];
  public userName: string;

  constructor(
    public menuService: MenuService,
    public imageService: ImageService,
    public utilsService: UtilsService,
    public settingsService: SettingsService,
    public httpMenuService: HttpMenuService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
  }

  ngOnInit() {
    super.ngOnInit();
    this.subscriptions.push(this.httpMenuService.getConnectionInfo().subscribe(result => {
      this.userName = result.user;
    }));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subs => subs.unsubscribe());
  }

  runFunction(object) {
    this.menuService.runFunction(object);
  }

}
