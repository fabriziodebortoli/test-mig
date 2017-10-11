import { Component, OnInit } from '@angular/core';

import { TabberService } from './../../core/services/tabber.service';
import { LocalizationService } from './../../core/services/localization.service';
import { ImageService } from './../../menu/services/image.service';
import { UtilsService } from './../../core/services/utils.service';
import { MenuService } from './../../menu/services/menu.service';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { SidenavService } from './../../core/services/sidenav.service';

@Component({
  selector: 'tb-home-sidenav',
  templateUrl: './home-sidenav.component.html',
  styleUrls: ['./home-sidenav.component.css']
})
export class HomeSidenavComponent implements OnInit {

  constructor(
    public sidenavService: SidenavService,
    public httpMenuService: HttpMenuService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public imageService: ImageService,
    public localizationService: LocalizationService,
    public tabberService: TabberService
  ) { }

  ngOnInit() { }

  toggleSidenav(menu: boolean = false) {
    
    this.sidenavService.toggleSidenav();

    if (menu) {
      this.tabberService.selectTab(1);
    }
  }

}
