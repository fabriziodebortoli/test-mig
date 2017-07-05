import { Component, OnInit, Input, OnChanges, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { SocketConnectionStatus } from "../../models";

import { LocalizationService } from './../../../menu/services/localization.service';
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
    private webSocketService: WebSocketService,
    private localizationService: LocalizationService) {

    this.subscriptions.push(localizationService.localizationsLoaded.subscribe(() => {
      this.localizationLoaded = true;
    }));
    this.subscriptions.push(this.webSocketService.connectionStatus.subscribe((status) => {
      this.status = status;
      this.connectionStatus = this.getConnectedStatusString(status);
      this.connectionStatusClass = this.getConnectionStatusClass(status);
    }));
  }

  private getConnectedStatusString(status: SocketConnectionStatus): string {
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

  private getConnectionStatusClass(status: SocketConnectionStatus): string {
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



