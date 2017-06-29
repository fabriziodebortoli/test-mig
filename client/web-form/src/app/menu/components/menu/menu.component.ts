import { Subscription } from 'rxjs';
import { title } from './../../../reporting-studio/reporting-studio.model';
import { EnumsService } from './../../../core/enums.service';
import { Component, OnInit, AfterContentInit, OnDestroy, Input, HostListener } from '@angular/core';

import { ViewModeType } from '../../../shared/models/view-mode-type.model';

import { EventDataService } from '@taskbuilder/core';

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

export class MenuComponent implements OnDestroy {

  @HostListener('window:beforeunload')
  onClose() {
    this.menuService.updateAllFavoritesAndMostUsed();
  }

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

  ngOnDestroy() {

    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
}
