import { ComponentService } from '../../core/services/component.service';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

import { DocumentComponent } from './../../shared/components/document.component';
import { DataService } from './../../core/services/data.service';
import { EventDataService } from './../../core/services/eventdata.service';

import {SettingsPageService} from '../settingsPage.service';


@Component({
  selector: 'tb-settings-container',
  templateUrl: './settings-container.component.html',
  styleUrls: ['./settings-container.component.scss'],
  providers: [DataService, EventDataService, SettingsPageService]
})
export class SettingsContainerComponent extends DocumentComponent implements OnInit {
  constructor(public eventData: EventDataService, private dataService: DataService, private settingsService: SettingsPageService ) { 
    super(settingsService,eventData, null);
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