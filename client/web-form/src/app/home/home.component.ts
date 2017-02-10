import { EventManagerService } from './../menu/services/event-manager.service';
import { SettingsService } from './../menu/services/settings.service';
import { LocalizationService } from './../menu/services/localization.service';
import { HttpMenuService } from './../menu/services/http-menu.service';
import { MenuService } from './../menu/services/menu.service';
import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from './../../environments/environment';

import { SidenavService, LoginSessionService, ComponentService } from './../core';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {

  @ViewChild('sidenav') sidenav;
  sidenavSubscription: any;
  getMenuElementsSubscription: any;

  constructor(
    private loginSession: LoginSessionService,
    private componentService: ComponentService,
    private sidenavService: SidenavService,
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private localizationService: LocalizationService,
    private settingsService: SettingsService,
    private eventManagerService: EventManagerService,
    private router: Router
  ) {
    this.sidenavSubscription = sidenavService.sidenavOpened$.subscribe(() => this.sidenav.toggle());

    this.eventManagerService.preferenceLoaded.subscribe(result => {
      this.menuService.initApplicationAndGroup(this.menuService.applicationMenu.Application);  //qui bisogna differenziare le app da caricare, potrebbero essere app o environment
    });

  }

  ngOnInit() {
    this.getMenuElementsSubscription = this.httpMenuService.getMenuElements().subscribe(result => {

      this.menuService.onAfterGetMenuElements(result.Root);

      this.localizationService.loadLocalizedElements(true);
      this.settingsService.getSettings();
    });
  }

  ngOnDestroy() {
    this.sidenavSubscription.unsubscribe();
    this.getMenuElementsSubscription.unsubscribe();
  }
}
