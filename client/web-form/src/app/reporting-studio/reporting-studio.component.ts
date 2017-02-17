import { ReportingStudioConnection } from './reporting-studio-connection.component';
import { MenuService } from './../menu/services/menu.service';

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-reporting-studio',
  templateUrl: './reporting-studio.component.html',
  styleUrls: ['./reporting-studio.component.scss']
})
export class ReportingStudioComponent implements OnInit, OnDestroy {

  sub: Subscription;
  private nameSpace: string;
  private rsConn: ReportingStudioConnection;

  private message: string = '';

  constructor(private menuService: MenuService) {
    this.nameSpace = menuService.nameSpace;
  }

  ngOnInit() {
    this.rsConn = new ReportingStudioConnection();

    this.sub = this.rsConn.message.subscribe(recieved => {
      this.onMessage(recieved);
    });

    this.rsConn.rsInitStateMachine(this.nameSpace);
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  onMessage(message: string) {
    //elaborate
    this.message = message;

  }

}
