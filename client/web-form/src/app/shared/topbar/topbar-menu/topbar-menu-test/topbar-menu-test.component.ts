import { ComponentService } from './../../../../core/component.service';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';
import { OpenComponent } from './../../../../shared/explorer/open/open.component';

@Component({
  selector: 'tb-topbar-menu-test',
  templateUrl: './topbar-menu-test.component.html',
  styleUrls: ['./topbar-menu-test.component.css']
})
export class TopbarMenuTestComponent implements OnInit {

  private title: string = "Test menu";

  constructor(private componentService: ComponentService, private resolver: ComponentFactoryResolver) { }

  ngOnInit() {
  }

  openDataService() {
    this.componentService.createComponentFromUrl('ds/dataservice');
  }

  openRS() {
   this.componentService.createComponentFromUrl('rs/reportingstudio/');
  }

  openTBExplorer(){
    this.componentService.createComponent(OpenComponent, this.resolver);
  }

}
