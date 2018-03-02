import { Component, ViewEncapsulation, ChangeDetectorRef, Output, EventEmitter, OnDestroy } from '@angular/core';

import { TbComponent } from './../../shared/components/tb.component';

import { Subscription } from '../../rxjs.imports';

import { TabberService } from './../../core/services/tabber.service';
import { ImageService } from './../../menu/services/image.service';
import { UtilsService } from './../../core/services/utils.service';
import { MenuService } from './../../menu/services/menu.service';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { SidenavService } from './../../core/services/sidenav.service';
import { TbComponentService } from './../../core/services/tbcomponent.service';

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
    public tabberService: TabberService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
    this.subscriptions.push(this.sidenavService.sidenavPinned$.subscribe((pinned) => this.sidenavPinned = pinned));
    this.subscriptions.push(this.sidenavService.sidenavOpened$.subscribe((opened) => this.sidenavOpened = opened));
  }

  itemSelected(){
    if(this.sidenavPinned) return;

    this.sidenavService.openedChange(false);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
}
