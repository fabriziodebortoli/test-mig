import { Subscription } from 'rxjs';
import { MaterialModule, MdDialog, MdDialogRef } from '@angular/material';
import { HttpMenuService } from './../../../services/http-menu.service';
import { LocalizationService } from './../../../services/localization.service';
import { Component, OnInit, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-connection-info-dialog',
  templateUrl: './connection-info-dialog.component.html',
  styleUrls: ['./connection-info-dialog.component.css']
})
export class ConnectionInfoDialogComponent implements OnInit, OnDestroy {

  private connectionInfos: any;
  private showdbsize: boolean;
  private connectionInfoSub: Subscription;
  constructor(
    public dialogRef: MdDialogRef<ConnectionInfoDialogComponent>,
    private httpMenuService: HttpMenuService,
    private localizationService: LocalizationService
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


