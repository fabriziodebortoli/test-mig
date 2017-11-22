import { Component, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { TestService } from './../test.service';

import { ComponentService } from './../../core/services/component.service';
import { DocumentComponent } from './../../shared/components/document.component';
import { EventDataService } from './../../core/services/eventdata.service';
import { DataService } from './../../core/services/data.service';

@Component({
  selector: 'tb-data-service',
  templateUrl: './data-service.component.html',
  styleUrls: ['./data-service.component.css'],
  providers: [DataService, EventDataService, TestService]
})
export class DataServiceComponent extends DocumentComponent implements OnInit {

  public nameSpace: string;
  public selection_type: string;
  public like_value: string;

  public disabled: string;
  public good_type: string;

  public responseData: any;
  public responseSelection: any;
  public responseParameters: any;
  public responseColumns: any;

  constructor(public eventData: EventDataService,
    public dataService: DataService,
    public http: Http,
    public testService: TestService,
    changeDetectorRef:ChangeDetectorRef) {
    super(testService, eventData, null, changeDetectorRef);
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