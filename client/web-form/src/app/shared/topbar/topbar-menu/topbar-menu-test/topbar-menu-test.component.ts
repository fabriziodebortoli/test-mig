import { ComponentService } from './../../../../core/component.service';
import { DataServiceComponent } from './../../../../applications/test/data-service/data-service.component';
import { ReportingStudioComponent } from './../../../../reporting-studio/reporting-studio.component';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';


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
    this.componentService.createComponent(DataServiceComponent, this.resolver);
  }

  openRS() {
    this.componentService.createComponent(ReportingStudioComponent, this.resolver);
  }

}
