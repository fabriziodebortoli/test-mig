import { LocalizationService } from './../../core/services/localization.service';
import { ComponentService } from '../../core/services/component.service';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

import { DocumentComponent } from './../../shared/components/document.component';
import { DataService } from './../../core/services/data.service';
import { EventDataService } from './../../core/services/eventdata.service';

import { SettingsPageService } from '../settingsPage.service';
import { InfoService } from './../../core/services/info.service';


@Component({
  selector: 'tb-settings-container',
  templateUrl: './settings-container.component.html',
  styleUrls: ['./settings-container.component.scss'],
  providers: [DataService, EventDataService, SettingsPageService]
})
export class SettingsContainerComponent extends DocumentComponent implements OnInit {

  isDesktop: boolean;

  constructor(
    public eventData: EventDataService, 
    public dataService: DataService, 
    public settingsService: SettingsPageService, 
    public infoService: InfoService,
    public localizationService: LocalizationService
  ) {
    super(settingsService, eventData, null);
    this.isDesktop = infoService.isDesktop;
  }
  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': 'Settings Page' } };
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