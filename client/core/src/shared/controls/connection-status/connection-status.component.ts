import { TaskBuilderService } from './../../../core/services/taskbuilder.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { Component, OnInit, Input, OnChanges, Output, EventEmitter, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { ConnectionStatus } from '../../models/connection-status.enum';

import { ControlComponent } from './../control.component';
import { TbComponent } from './../../components/tb.component';

@Component({
  selector: 'tb-connection-status',
  templateUrl: './connection-status.component.html',
  styleUrls: ['./connection-status.component.scss']
})

export class ConnectionStatusComponent extends TbComponent implements OnDestroy {
  subscriptions: Subscription[] = [];
  connectionStatus = '';
  status = ConnectionStatus.None;
  connectionStatusClass = this.getConnectionStatusClass();
  constructor(
    public tbService: TaskBuilderService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);

    this.enableLocalization();
    this.subscriptions.push(this.tbService.connectionStatus.subscribe((status) => {
      this.status = status;
      this.connectionStatus = this.getConnectedStatusString();
      this.connectionStatusClass = this.getConnectionStatusClass();
    }));
  }

  public getConnectedStatusString(): string {

    switch (this.status) {
      case ConnectionStatus.Connected:
        return this._TB('Connected');
      case ConnectionStatus.Connecting:
        return this._TB('Connecting');
      case ConnectionStatus.Unavailable:
        return this._TB('Backend services not available; clic to try to reconnect');
      case ConnectionStatus.Disconnected:
      case ConnectionStatus.None:
        return this._TB('Disconnected');
      default:
        return "";
    }
  }

  public getConnectionStatusClass(): string {
    switch (this.status) {
      case ConnectionStatus.Connected:
        return 'connected';
      case ConnectionStatus.Connecting:
        return 'connecting';
      case ConnectionStatus.Unavailable:
        return 'unavailable';
      case ConnectionStatus.Disconnected:
      case ConnectionStatus.None:
        return 'disconnected';
    }
  }
  ngOnDestroy() {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
  reconnect() {
    if (this.tbService._connectionStatus == ConnectionStatus.Unavailable) {
      this.tbService.openConnection();
    }

  }
}




