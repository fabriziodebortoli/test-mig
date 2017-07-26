import { TestService } from './../test.service';
import { EventDataService } from './../../core/services/eventdata.service';
import { DocumentComponent } from './../../shared/components/document.component';
import { ComponentService } from './../../core/services/component.service';
import { Component, ComponentFactoryResolver, OnInit } from '@angular/core';

@Component({
  selector: 'tb-tree-test',
  templateUrl: './tree-test.component.html',
  styleUrls: ['./tree-test.component.scss'],
  providers: [EventDataService, TestService]

})
export class TreeTestComponent extends DocumentComponent implements OnInit {
  constructor(public eventData: EventDataService, private testService: TestService) {
    super(testService, eventData);
  }

 ngOnInit(){
    this.eventData.model = { 'Title': { 'value': 'Tree Test' } };
  }
}



// ------------------------------------
@Component({
  template: ''
})
export class TreeTestFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(TreeTestComponent, resolver);
  }
} 