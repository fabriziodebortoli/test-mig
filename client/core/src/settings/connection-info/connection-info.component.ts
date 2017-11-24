import { LocalizationService } from './../../core/services/localization.service';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from '../../rxjs.imports';

@Component({
  selector: 'tb-connection-info',
  templateUrl: './connection-info.component.html',
  styleUrls: ['./connection-info.component.scss']
})
export class ConnectionInfoComponent implements OnInit, OnDestroy {

  public connectionInfos: any;

  private subscriptions = [];
  constructor(
    public httpMenuService: HttpMenuService,
    public localizationService: LocalizationService
  ) {

  }

  ngOnInit() {
    this.subscriptions.push(this.httpMenuService.getConnectionInfo().subscribe(result => {
      this.connectionInfos = result;
      
    }));

    this.subscriptions.push(this.localizationService.localizationsLoaded.subscribe((loaded) => {
      if (!loaded)
        return;
    }));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subs => subs.unsubscribe());
  }
}


