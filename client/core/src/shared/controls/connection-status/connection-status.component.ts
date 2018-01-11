import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { Component, OnInit, Input, OnChanges, Output, EventEmitter, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { SocketConnectionStatus } from '../../models/websocket-connection.enum';
import { WebSocketService } from './../../../core/services/websocket.service';

import { ControlComponent } from './../control.component';
import { TbComponent } from './../../components/tb.component';

@Component({
  selector: 'tb-connection-status',
  templateUrl: './connection-status.component.html',
  styleUrls: ['./connection-status.component.scss']
})

export class ConnectionStatusComponent extends TbComponent implements OnDestroy {
  subscriptions: Subscription[] = [];
  connectionStatus: string = "";
  connectionStatusClass: string = "disconnected";
  status: SocketConnectionStatus = SocketConnectionStatus.None;
  constructor(
    public webSocketService: WebSocketService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);

    this.enableLocalization();
    this.subscriptions.push(this.webSocketService.connectionStatus.subscribe((status) => {
      this.status = status;
      this.connectionStatus = this.getConnectedStatusString(status);
      this.connectionStatusClass = this.getConnectionStatusClass(status);
    }));
  }

  public getConnectedStatusString(status: SocketConnectionStatus): string {

    switch (status) {
      case SocketConnectionStatus.Connected:
        return this._TB('Connected');
      case SocketConnectionStatus.Connecting:
        return this._TB('Connecting');
      case SocketConnectionStatus.Disconnected:
        return this._TB('Disconnected');
      default:
        return "";
    }
  }

  public getConnectionStatusClass(status: SocketConnectionStatus): string {
    switch (status) {
      case SocketConnectionStatus.Connected:
        return "connected";
      case SocketConnectionStatus.Connecting:
        return "connecting";
      case SocketConnectionStatus.Disconnected:
        return "disconnected";
      default:
        return "";
    }
  }
  ngOnDestroy() {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
}




