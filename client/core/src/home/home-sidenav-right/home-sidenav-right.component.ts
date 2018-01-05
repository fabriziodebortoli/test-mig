import { TbComponentService } from './../../core/services/tbcomponent.service';
import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { TabberService } from './../../core/services/tabber.service';
import { ImageService } from './../../menu/services/image.service';
import { UtilsService } from './../../core/services/utils.service';
import { MenuService } from './../../menu/services/menu.service';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { SidenavService } from './../../core/services/sidenav.service';
import { TbComponent } from './../../shared/components/tb.component';

@Component({
  selector: 'tb-home-sidenav-right',
  templateUrl: './home-sidenav-right.component.html',
  styleUrls: ['./home-sidenav-right.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class HomeSidenavRightComponent extends TbComponent  {

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
  }

  toggleSidenavRight() {
    this.sidenavService.toggleSidenavRight();
  }
}
