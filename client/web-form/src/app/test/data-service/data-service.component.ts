import { URLSearchParams, Http } from '@angular/http';
import { environment } from './../../../environments/environment';
import { EventDataService } from './../../core/eventdata.service';
import { ComponentService, DataService } from 'tb-core';
import { DocumentComponent } from 'tb-shared';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

@Component({
  selector: 'tb-data-service',
  templateUrl: './data-service.component.html',
  styleUrls: ['./data-service.component.css'],
  providers: [DataService, EventDataService]
})
export class DataServiceComponent extends DocumentComponent implements OnInit {

  private nameSpace: string = 'erp.items.ds_ItemsSimple';
  private selection_type: string = 'Core';
  private like_value: string = '';
  private disabled: string = '';
  private good_type: string = '';



  constructor(public eventData: EventDataService, private dataService: DataService, private http: Http) {
    super(dataService, eventData);

    /*httpService.postData(httpService.getBaseUrl() + 'ds/data-service', {}).map((res: Response) => {
               return res.ok && res.json().success === true;
    }); */
  }

  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': this.nameSpace } };
  }

  SendData() {
    let url: string = environment.baseUrl + 'data-service/getdata/' + this.nameSpace;

    let params: URLSearchParams = new URLSearchParams();
    params.set('selection_type', this.selection_type);
    params.set(' like_value', this.like_value);
    params.set('disabled', this.disabled);
    params.set('good_type', this.good_type);

    let subs = this.http.get(url, { search: params }).subscribe(res => {
      // handle Response
      subs.unsubscribe();
    });
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