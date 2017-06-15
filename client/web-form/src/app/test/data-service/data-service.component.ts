import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { environment } from './../../../environments/environment';

import { DocumentComponent } from '../../shared/document.component';

import { ComponentService } from './../../core/component.service';
import { EventDataService } from './../../core/eventdata.service';
import { DataService } from './../../core/data.service';

@Component({
  selector: 'tb-data-service',
  templateUrl: './data-service.component.html',
  styleUrls: ['./data-service.component.css'],
  providers: [DataService, EventDataService]
})
export class DataServiceComponent extends DocumentComponent implements OnInit {

  private nameSpace: string;
  private selection_type: string;
  private like_value: string;

  private disabled: string;
  private good_type: string;

  private responseData: any;
  private responseSelection: any;
  private responseParameters: any;
  private responseColumns: any;

  constructor(public eventData: EventDataService, private dataService: DataService, private http: Http) {
    super(dataService, eventData);
  }

  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': this.nameSpace } };

    this.nameSpace = 'erp.items.dbl.ds_ItemsSimple';
    this.selection_type = 'code';
    this.like_value = '%';
    this.disabled = '2';
    this.good_type = '2';
  }

  GetData() {

    let params: URLSearchParams = new URLSearchParams();
    // params.set('selection_type', this.selection_type);
    params.set('like_value', this.like_value);
    params.set('disabled', this.disabled);
    params.set('good_type', this.good_type);

    let subs = this.dataService.getData(this.nameSpace, this.selection_type, params).subscribe(data => {
      this.responseData = data;
      subs.unsubscribe();
    });
  }

  GetColumns() {

    let subs = this.dataService.getColumns(this.nameSpace, this.selection_type).subscribe(data => {
      this.responseColumns = data;
      subs.unsubscribe();
    });
  }

  GetSelections() {

    let subs = this.dataService.getSelections(this.nameSpace).subscribe(data => {
      this.responseSelection = data;
      subs.unsubscribe();
    });
  }

 GetParameters() {

    let subs = this.dataService.getParameters(this.nameSpace).subscribe(data => {
      this.responseParameters = data;
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