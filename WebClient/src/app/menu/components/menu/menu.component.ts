import { LocalizationService } from './../../services/localization.service';
import { MenuService } from './../../services/menu.service';
import { EventManagerService } from './../../services/event-manager.service';
import { SettingsService } from './../../services/settings.service';
import { HttpMenuService } from './../../services/http-menu.service';
import { UtilsService } from 'tb-core';
import { DocumentInfo } from 'tb-shared';
import { RightSidenavComponent } from './../../components/menu/sidenav-right-content/sidenav-right-content.component';
import { LeftSidenavComponent } from './../../components/menu/sidenav-left-content/sidenav-left-content.component';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})

export class MenuComponent implements OnInit {

  private menu: undefined;
  private applications: undefined;

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private settingsService: SettingsService,
    private localizationService: LocalizationService,
    private eventManagerService: EventManagerService
  ) {
    this.eventManagerService.preferenceLoaded.subscribe(result => {
      menuService.initApplicationAndGroup(this.menuService.applicationMenu.Application);  //qui bisogna differenziare le app da caricare, potrebbero essere app o environment
    }
    );

  }
  ngOnInit() {

    this.httpMenuService.getMenuElements().subscribe(result => {
      this.menuService.applicationMenu = result.Root.ApplicationMenu.AppMenu;
      this.menuService.environmentMenu = result.Root.EnvironmentMenu.AppMenu;
      this.menuService.loadFavoritesAndMostUsed();
      this.localizationService.loadLocalizedElements(true);
      this.settingsService.getSettings();
    });
  }

  runDocument(ns: string) {
    this.httpMenuService.runObject(new DocumentInfo(0, ns, this.utilsService.generateGUID()));
  }
}
