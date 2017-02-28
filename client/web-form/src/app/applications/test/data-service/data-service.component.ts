import { ComponentService } from 'tb-core';
import { DocumentComponent } from 'tb-shared';
import { EventDataService } from './../../../core/eventdata.service';
import { DataServiceService } from './data-service.service';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

@Component({
  selector: 'tb-data-service',
  templateUrl: './data-service.component.html',
  styleUrls: ['./data-service.component.css'],
  providers: [DataServiceService, EventDataService/*, DocumentService*/]
})
export class DataServiceComponent extends DocumentComponent implements OnInit {

  private nameSpace: string = 'erp.items.ds_ItemsSimple';
  private selection_type: string = 'Core';
  private like_value: string;
  private disabled: string;
  private good_type: string;

  constructor(public eventData: EventDataService, private dataService: DataServiceService) {
    super(dataService, eventData);

    /*httpService.postData(httpService.getBaseUrl() + 'ds/data-service', {}).map((res: Response) => {
               return res.ok && res.json().success === true;
    }); */
  } 

  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': this.nameSpace } };
  }

}

@Component({
    template: ''
})
export class DataServiceFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(DataServiceComponent, resolver);
    }
} 