import { Subscription } from 'rxjs';
import { title } from './../../../reporting-studio/reporting-studio.model';
import { EnumsService } from './../../../core/enums.service';
import { Component, OnInit, OnDestroy, Input } from '@angular/core';

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

  private getMenuElementsSubscription: Subscription;
  private selectedGroupChangedSubscription: Subscription;

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private localizationService: LocalizationService,
    private settingsService: SettingsService,
    private eventManagerService: EventManagerService,
    private eventData: EventDataService,
    private enumsService: EnumsService
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

    this.selectedGroupChangedSubscription = this.menuService.selectedGroupChanged.subscribe((title) => {
      this.eventData.model.Title.value = title + ' Menu';
    });
  }

  ngOnInit() {
    this.getMenuElementsSubscription = this.httpMenuService.getMenuElements().subscribe(result => {
      this.menuService.onAfterGetMenuElements(result.Root);
      this.localizationService.loadLocalizedElements(true);
      this.settingsService.getSettings();
      this.enumsService.getEnumsTable();
    });
  }

  ngOnDestroy() {
    this.getMenuElementsSubscription.unsubscribe();
    this.selectedGroupChangedSubscription.unsubscribe();
  }
}
