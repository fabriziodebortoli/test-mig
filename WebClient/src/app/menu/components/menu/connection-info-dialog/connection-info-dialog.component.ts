
import { MaterialModule, MdDialog, MdDialogRef } from '@angular/material';
import { HttpMenuService } from './../../../services/http-menu.service';
import { MenuService } from './../../../services/menu.service';
import { UtilsService } from 'tb-core';
import { LocalizationService } from './../../../services/localization.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-connection-info-dialog',
  templateUrl: './connection-info-dialog.component.html',
  styleUrls: ['./connection-info-dialog.component.css']
})
export class ConnectionInfoDialogComponent implements OnInit {

  private connectionInfos: any;
  private showdbsize: boolean;

  constructor(
    public dialogRef: MdDialogRef<ConnectionInfoDialogComponent>,
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private localizationService: LocalizationService
  ) {

  }

  ngOnInit() {
    this.httpMenuService.getConnectionInfo().subscribe(result => {
      this.connectionInfos = result;
      this.showdbsize = this.connectionInfos.showdbsizecontrols == 'Yes';

      // if (result.messages)
      // 	$scope.loggingService.showDiagnostic(
      // 		data.messages,
      // 		{ onOk: function () { callback(data); } }
      // 		);
    });
  }
}


