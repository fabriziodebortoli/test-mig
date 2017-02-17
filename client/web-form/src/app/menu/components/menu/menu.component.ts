import { EventDataService } from 'tb-core';
import { ViewModeType } from 'tb-shared';
import { EventManagerService } from './../../services/event-manager.service';
import { SettingsService } from './../../services/settings.service';
import { LocalizationService } from './../../services/localization.service';
import { MenuService } from './../../services/menu.service';
import { HttpMenuService } from './../../services/http-menu.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})

export class MenuComponent implements OnInit {
  getMenuElementsSubscription: any;
  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private localizationService: LocalizationService,
    private settingsService: SettingsService,
    private eventManagerService: EventManagerService
  ) {
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
    this.getMenuElementsSubscription.unsubscribe();
  }
}
