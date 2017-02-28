import { ComponentService } from './../../../../core/component.service';
import { ReportingStudioComponent } from './../../../../reporting-studio/reporting-studio.component';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

//import { DataServiceComponent } from './../../../../applications/test/data-service/data-service.component';
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
    //this.componentService.createComponent(DataServiceComponent, this.resolver);
    this.componentService.createComponentFromUrl('ds/dataservice');
  }

  openRS() {
    this.componentService.createComponent(ReportingStudioComponent, this.resolver);
  }

  openTBExplorer(){
    this.componentService.createComponent(OpenComponent, this.resolver);
  }

}
