import { Component, OnInit, AfterContentInit, OnDestroy, Input, HostListener } from '@angular/core';

import { Subscription } from 'rxjs/Rx';

import { EventDataService, ComponentInfoService, MenuService, SettingsService, EventManagerService, EnumsService, ViewModeType } from '@taskbuilder/core';

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
