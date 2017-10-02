import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { MaterialModule, MdDialog, MdDialogRef } from '@angular/material';

import { LocalizationService } from './../../../services/localization.service';
import { HttpMenuService } from './../../../services/http-menu.service';

@Component({
  selector: 'tb-connection-info-dialog',
  templateUrl: './connection-info-dialog.component.html',
  styleUrls: ['./connection-info-dialog.component.css']
})
export class ConnectionInfoDialogComponent implements OnInit, OnDestroy {

  public connectionInfos: any;
  public showdbsize: boolean;
  public connectionInfoSub: Subscription;

  constructor(
    public dialogRef: MdDialogRef<ConnectionInfoDialogComponent>,
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


