import { Component, OnInit, Input, OnChanges, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { SocketConnectionStatus } from '../../models/websocket-connection.enum';
import { OldLocalizationService } from './../../../core/services/oldlocalization.service';
import { WebSocketService } from './../../../core/services/websocket.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-connection-status',
  templateUrl: './connection-status.component.html',
  styleUrls: ['./connection-status.component.scss']
})

export class ConnectionStatusComponent implements OnDestroy {
  subscriptions: Subscription[] = [];
  localizationLoaded: boolean = false;
  connectionStatus: string = "";
  connectionStatusClass: string = "disconnected";
  status: SocketConnectionStatus = SocketConnectionStatus.None;
  constructor(
    public webSocketService: WebSocketService,
    public localizationService: OldLocalizationService
  ) {

    this.subscriptions.push(localizationService.localizationsLoaded.subscribe((loaded) => {
      this.localizationLoaded = loaded;
    }));
    this.subscriptions.push(this.webSocketService.connectionStatus.subscribe((status) => {
      this.status = status;
      this.connectionStatus = this.getConnectedStatusString(status);
      this.connectionStatusClass = this.getConnectionStatusClass(status);
    }));
  }

  public getConnectedStatusString(status: SocketConnectionStatus): string {
    if (!this.localizationLoaded)
      return "";

    switch (status) {
      case SocketConnectionStatus.Connected:
        return this.localizationService.localizedElements.Connected;
      case SocketConnectionStatus.Connecting:
        return this.localizationService.localizedElements.Connecting;
      case SocketConnectionStatus.Disconnected:
        return this.localizationService.localizedElements.Disconnected;
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




