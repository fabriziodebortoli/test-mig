import { EventDataService } from './../../core/eventdata.service';
import { DocumentComponent } from 'tb-shared';
import { ComponentService, ExplorerService } from 'tb-core';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

@Component({
  selector: 'tb-explorer',
  templateUrl: './explorer.component.html',
  styleUrls: ['./explorer.component.scss'],
  providers: [ExplorerService, EventDataService]
})
export class ExplorerComponent extends DocumentComponent implements OnInit {

  constructor(public eventData: EventDataService, private explorerService: ExplorerService) {
    super(explorerService, eventData);
  }

  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': "Explorer Component Demo" } };
  }

}

@Component({
  template: ''
})
export class ExplorerFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(ExplorerComponent, resolver);
  }
} 