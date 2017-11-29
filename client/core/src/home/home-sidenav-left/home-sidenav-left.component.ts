import { Component, OnInit, ViewEncapsulation } from '@angular/core';

import { TabberService } from './../../core/services/tabber.service';
import { OldLocalizationService } from './../../core/services/oldlocalization.service';
import { ImageService } from './../../menu/services/image.service';
import { UtilsService } from './../../core/services/utils.service';
import { MenuService } from './../../menu/services/menu.service';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { SidenavService } from './../../core/services/sidenav.service';

@Component({
  selector: 'tb-home-sidenav-left',
  templateUrl: './home-sidenav-left.component.html',
  styleUrls: ['./home-sidenav-left.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class HomeSidenavLeftComponent implements OnInit {

  constructor(
    public sidenavService: SidenavService,
    public httpMenuService: HttpMenuService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public imageService: ImageService,
    public localizationService: OldLocalizationService,
    public tabberService: TabberService
  ) { }

  ngOnInit() { }

  toggleSidenavLeft(menu: boolean = false) {
    
    this.sidenavService.toggleSidenavLeft();

    if (menu) {
      this.tabberService.selectMenuTab();
    }
  }
}
