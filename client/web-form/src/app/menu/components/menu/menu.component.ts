import { Component, OnInit, OnDestroy } from '@angular/core';

import { ViewModeType } from '../../../shared/models/view-mode-type.model';

import { EventDataService } from './../../../core/eventdata.service';

import { EventManagerService } from './../../services/event-manager.service';
import { SettingsService } from './../../services/settings.service';
import { LocalizationService } from './../../services/localization.service';
import { MenuService } from './../../services/menu.service';
import { HttpMenuService } from './../../services/http-menu.service';

@Component({
  selector: 'tb-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss'],
  providers: [EventDataService]
})

export class MenuComponent implements OnInit, OnDestroy {
  getMenuElementsSubscription: any;
  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private localizationService: LocalizationService,
    private settingsService: SettingsService,
    private eventManagerService: EventManagerService,
    private eventData: EventDataService
  ) {
    this.eventManagerService.preferenceLoaded.subscribe(result => {
      this.menuService.initApplicationAndGroup(this.menuService.applicationMenu.Application);  //qui bisogna differenziare le app da caricare, potrebbero essere app o environment
    });

    this.eventData.model = {
      Title: {
        value: 'Menu'
      },
      viewModeType: ViewModeType.M
    };
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
