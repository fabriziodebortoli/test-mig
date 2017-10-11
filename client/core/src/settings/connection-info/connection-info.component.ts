import { LocalizationService } from './../../core/services/localization.service';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { MaterialModule, MdDialog, MdDialogRef } from '@angular/material';

@Component({
  selector: 'tb-connection-info',
  templateUrl: './connection-info.component.html',
  styleUrls: ['./connection-info.component.scss']
})
export class ConnectionInfoComponent implements OnInit, OnDestroy {

  public connectionInfos: any;
  public showdbsize: boolean;
  public connectionInfoSub: Subscription;

  constructor(
    //public dialogRef: MdDialogRef<ConnectionInfoDialogComponent>,
    public httpMenuService: HttpMenuService,
    public localizationService: LocalizationService
  ) {

  }

  ngOnInit() {
    this.connectionInfoSub = this.httpMenuService.getConnectionInfo().subscribe(result => {
      this.connectionInfos = result;
      this.showdbsize = this.connectionInfos.showdbsizecontrols == 'Yes';
    });
  }

  ngOnDestroy() {
    this.connectionInfoSub.unsubscribe();
  }
}


