import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';

import { TbComponent, InfoService, TbComponentService } from '@taskbuilder/core';

import { HttpMenuService } from './../../services/http-menu.service';

@Component({
  selector: 'tb-connection-info',
  templateUrl: './connection-info.component.html',
  styleUrls: ['./connection-info.component.scss']
})
export class ConnectionInfoComponent extends TbComponent implements OnInit, OnDestroy {

  public connectionInfos: any;

  private subscriptions = [];

  constructor(
    public httpMenuService: HttpMenuService,
    public infoService: InfoService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
  }

  ngOnInit() {
    super.ngOnInit();
    this.subscriptions.push(this.httpMenuService.getConnectionInfo().subscribe(result => {
      this.connectionInfos = result;

    }));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subs => subs.unsubscribe());
  }
}


