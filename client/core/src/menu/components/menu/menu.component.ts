import { EventManagerService } from './../../../core/services/event-manager.service';
import { SettingsService } from './../../../core/services/settings.service';
import { Component, OnInit, AfterContentInit, OnDestroy, Input, HostListener } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { OldLocalizationService } from './../../../core/services/oldlocalization.service';
import { ComponentInfoService } from './../../../core/services/component-info.service';
import { HttpMenuService } from './../../services/http-menu.service';
import { MenuService } from './../../services/menu.service';
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

  public subscriptions: Subscription[] = [];
  constructor(
    public menuService: MenuService,
    public localizationService: OldLocalizationService,
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
  }

  ngOnDestroy() {

    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
}
