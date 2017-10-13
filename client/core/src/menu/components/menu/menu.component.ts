import { Component, OnInit, AfterContentInit, OnDestroy, Input, HostListener } from '@angular/core';
import { Subscription } from 'rxjs';

import { LocalizationService } from './../../../core/services/localization.service';
import { ComponentInfoService } from './../../../core/services/component-info.service';
import { HttpMenuService } from './../../services/http-menu.service';
import { MenuService } from './../../services/menu.service';
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

  public subscriptions: Subscription[] = [];
  constructor(
    public menuService: MenuService,
    public localizationService: LocalizationService,
    public settingsService: SettingsService,
    public eventManagerService: EventManagerService,
    public eventData: EventDataService,
    public enumsService: EnumsService
  ) {
    this.eventData.model = {
      Title: {
        value: 'Menu'
      },
      viewModeType: ViewModeType.M
    };

    this.subscriptions.push(this.menuService.selectedGroupChanged.subscribe((title) => {
      this.eventData.model.Title.value = this.menuService.selectedApplication.title + ' - ' + title;
    }));

    this.subscriptions.push(this.eventManagerService.loggingOff.subscribe((res) => {
      this.menuService.updateAllFavoritesAndMostUsed();
    }));
  }

  ngOnDestroy() {

    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
}
