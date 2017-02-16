import { ReportingStudioConnection } from './reporting-studio-connection.component';
import { MenuService } from './../menu/services/menu.service';

import { Component, OnInit, OnDestroy } from '@angular/core';



@Component({
  selector: 'app-reporting-studio',
  templateUrl: './reporting-studio.component.html',
  styleUrls: ['./reporting-studio.component.scss']
})
export class ReportingStudioComponent implements OnInit, OnDestroy {

  private nameSpace: string;
  private rsConn: ReportingStudioConnection;

  constructor(private menuService: MenuService) {
    this.nameSpace = menuService.nameSpace;
  }

  ngOnInit() {
    this.rsConn = new ReportingStudioConnection();
    this.rsConn.rsInitStateMachine(this.nameSpace);
  }

  ngOnDestroy() {

  }

}
