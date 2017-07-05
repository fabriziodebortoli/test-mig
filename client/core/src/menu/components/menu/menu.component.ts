import { Component, OnInit, AfterContentInit, OnDestroy, Input, HostListener } from '@angular/core';
import { Subscription } from 'rxjs';

import { HttpMenuService } from './../../services/http-menu.service';
import { MenuService } from './../../services/menu.service';
import { LocalizationService } from './../../services/localization.service';
import { SettingsService } from './../../services/settings.service';
import { EventManagerService } from './../../services/event-manager.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { EnumsService } from './../../../core/services/enums.service';
import { ViewModeType } from './../../../shared/models/view-mode-type.model';

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
