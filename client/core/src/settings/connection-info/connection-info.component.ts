import { TbComponent } from './../../shared/components/tb.component';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { TbComponentService } from './../../core/services/tbcomponent.service';

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


