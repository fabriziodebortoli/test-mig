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

  model:any;
  private nodes = [];
  constructor(public eventData: EventDataService, private testService: TestService) {
    super(testService, eventData, null);
  }

 ngOnInit(){
    this.eventData.model = { 'Title': { 'value': 'Tree Test' } };
     for (let i = 1; i <= 3; i++) {
      let node = {
        id: i,
        value: 'Bath ' + i,
        icon: 'fa fa-bath',
        children: []
      };


      for (let y = 1; y <= 3; y++) {
        let child = {
          id: i + '' + y,
          icon: 'fa fa-shower',
          value: node.value + ' Shower ' + y,
        };
        node.children.push(child);

      }
      this.nodes.push(node);
    }

    this.nodes = [{
      id: 0,
      value: 'Bathroom',
      icon: 'fa fa-trophy',
      children: this.nodes
    }];

    this.model={
      value: this.nodes
    }
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