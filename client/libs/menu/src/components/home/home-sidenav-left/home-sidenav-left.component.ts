import { Component, ViewEncapsulation, ChangeDetectorRef, Output, EventEmitter, OnDestroy } from '@angular/core';

import { Subscription } from 'rxjs';

import { TbComponent, SidenavService, UtilsService, TbComponentService } from '@taskbuilder/core';

import { ImageService } from './../../../services/image.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';

@Component({
  selector: 'tb-home-sidenav-left',
  templateUrl: './home-sidenav-left.component.html',
  styleUrls: ['./home-sidenav-left.component.scss']
})
export class HomeSidenavLeftComponent extends TbComponent implements OnDestroy {

  subscriptions: Subscription[] = [];
  sidenavPinned: boolean = false;
  sidenavOpened: boolean = false;

  constructor(
    public sidenavService: SidenavService,
    public httpMenuService: HttpMenuService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public imageService: ImageService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
    this.subscriptions.push(this.sidenavService.sidenavPinned$.subscribe((pinned) => this.sidenavPinned = pinned));
    this.subscriptions.push(this.sidenavService.sidenavOpened$.subscribe((opened) => this.sidenavOpened = opened));
  }

  itemSelected() {
    if (this.sidenavPinned) return;

    this.sidenavService.openedChange(false);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
}
