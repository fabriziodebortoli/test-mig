import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { Subscription } from 'rxjs';


import { ViewModeType } from '../../../shared/models/view-mode-type.model';

import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';

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

  private subscriptions: Subscription[] = [];
  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private localizationService: LocalizationService,
    private settingsService: SettingsService,
    private eventManagerService: EventManagerService,
    private eventData: EventDataService,
    private enumsService: EnumsService
  ) {
    this.subscriptions.push(this.eventManagerService.preferenceLoaded.subscribe(result => {
      this.menuService.initApplicationAndGroup(this.menuService.applicationMenu.Application);  //qui bisogna differenziare le app da caricare, potrebbero essere app o environment

    }));

    this.eventData.model = {
      Title: {
        value: 'Menu'
      },
      viewModeType: ViewModeType.M
    };

    this.subscriptions.push(this.menuService.selectedGroupChanged.subscribe((title) => {
      this.eventData.model.Title.value = title + ' Menu';
    }));
  }

  ngOnInit() {
    this.subscriptions.push(this.httpMenuService.getMenuElements().subscribe(result => {
      this.menuService.onAfterGetMenuElements(result.Root);
      this.localizationService.loadLocalizedElements(true);
      this.settingsService.getSettings();
      this.enumsService.getEnumsTable();
    }));
  }

  ngOnDestroy() {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
}
