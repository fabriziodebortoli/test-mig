import { EnumsService } from './../../core/enums.service';
import { UtilsService, TabberService, SidenavService } from '@taskbuilder/core';
import { ImageService } from '@taskbuilder/core';
import { HttpMenuService } from '@taskbuilder/core';
import { MenuService } from '@taskbuilder/core';
import { LocalizationService } from '@taskbuilder/core';
import { Component, OnInit } from '@angular/core';

import { environment } from '../../../environments/environment';

@Component({
  selector: 'tb-home-sidenav',
  templateUrl: './home-sidenav.component.html',
  styleUrls: ['./home-sidenav.component.css']
})
export class HomeSidenavComponent implements OnInit {

  private appName = environment.appName;
  private companyName = environment.companyName;

  constructor(
    private sidenavService: SidenavService,
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService,
    private localizationService: LocalizationService,
    private tabberService: TabberService
  ) {


  }

  ngOnInit() {
  }

  toggleSidenav(menu: boolean = false) {
    this.sidenavService.toggleSidenav();

    if (menu) {
      this.tabberService.selectTab(1);
    }
  }

}
