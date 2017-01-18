import { ImageService } from './../../menu/services/image.service';
import { UtilsService } from 'tb-core';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { MenuService } from './../../menu/services/menu.service';
import { LocalizationService } from './../../menu/services/localization.service';
import { SidenavService } from './../../core/sidenav.service';
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

  constructor(private sidenavService: SidenavService, 
      private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService, 
    private localizationService: LocalizationService
  ) { }

  ngOnInit() {
  }

  toggleSidenav() {
    this.sidenavService.toggleSidenav();
  }

}
