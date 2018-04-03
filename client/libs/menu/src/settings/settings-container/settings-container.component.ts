import { Component, OnInit, ComponentFactoryResolver, ChangeDetectorRef, ViewChild } from '@angular/core';

import { DataService, EventDataService, DocumentComponent, InfoService, HttpMenuService, ComponentService } from '@taskbuilder/core';

import { SettingsPageService } from '../settings-page.service';

@Component({
  selector: 'tb-settings-container',
  templateUrl: './settings-container.component.html',
  styleUrls: ['./settings-container.component.scss'],
  providers: [DataService, EventDataService, SettingsPageService]
})
export class SettingsContainerComponent extends DocumentComponent implements OnInit {

  isDesktop: boolean;


  constructor(
    eventData: EventDataService,
    public dataService: DataService,
    settingsPageService: SettingsPageService,
    public infoService: InfoService,
    changeDetectorRef: ChangeDetectorRef,
    public httpMenuService: HttpMenuService
  ) {
    super(settingsPageService, eventData, null, changeDetectorRef);
    this.enableLocalization();
    this.isDesktop = infoService.isDesktop;
  }

  ngOnInit() {
    super.ngOnInit();
    this.eventData.model = { 'Title': { 'value': this._TB('Settings Page') } };
  }

  goToSite() {
    let subs = this.httpMenuService.goToSite().subscribe((result) => {
      subs.unsubscribe();
      window.open(result.url, "_blank");

    });
  }

  activateViaSMS() {
    let subs = this.httpMenuService.activateViaSMS().subscribe((result) => {
      subs.unsubscribe();
      window.open(result.url, "_blank");
    });

  }




}
@Component({
  template: ''
})
export class SettingsContainerFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(SettingsContainerComponent, resolver);
  }
} 