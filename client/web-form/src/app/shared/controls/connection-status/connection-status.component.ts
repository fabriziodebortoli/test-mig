import { Subscription } from 'rxjs';
import { LocalizationService } from './../../../menu/services/localization.service';
import { LoginSessionService } from './../../../core/login-session.service';
import { ControlComponent } from './../control.component';
import { Component, OnInit, Input, OnChanges, Output, EventEmitter, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-connection-status',
  templateUrl: './connection-status.component.html',
  styleUrls: ['./connection-status.component.scss']
})

export class ConnectionStatusComponent implements OnDestroy {
  localizationsLoadedSubscription: Subscription;
  localizationLoaded: boolean = false;
  constructor(
    private loginSession: LoginSessionService,
    private localizationService: LocalizationService) {

    this.localizationsLoadedSubscription = localizationService.localizationsLoaded.subscribe(() => { this.localizationLoaded = true; });
  }


  getConnectionStatus() {
   
    if (!this.localizationLoaded)
      return "";

    return this.loginSession.connected ? this.localizationService.localizedElements.Connected : this.localizationService.localizedElements.Disconnected;
  }

  ngOnDestroy()
  {
     this.localizationsLoadedSubscription.unsubscribe();
  }
}




