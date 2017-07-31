import { ComponentInfoService } from './../../../shared/models/component-info.model';
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
  providers: [EventDataService, ComponentInfoService]
})

export class MenuComponent implements OnDestroy {

  @HostListener('window:beforeunload', ['$event'])
  onClose($event) {
    this.menuService.updateAllFavoritesAndMostUsed();
  }

  private subscriptions: Subscription[] = [];
  constructor(
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

    this.subscriptions.push(this.eventManagerService.loggingOff.subscribe((res) => {
      this.menuService.updateAllFavoritesAndMostUsed();
    }));
  }

  ngOnDestroy() {

    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
}
