import { LocalizationService } from './../../menu/services/localization.service';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'tb-connection-info',
  templateUrl: './connection-info.component.html',
  styleUrls: ['./connection-info.component.scss']
})
export class ConnectionInfoComponent implements OnInit, OnDestroy {

  public connectionInfos: any;
  public showdbsize: boolean;
  public connectionInfoSub: Subscription;

  constructor(
    public httpMenuService: HttpMenuService,
    public localizationService: LocalizationService
  ) {

  }

  ngOnInit() {
    this.connectionInfoSub = this.httpMenuService.getConnectionInfo().subscribe(result => {
      this.connectionInfos = result;
      this.showdbsize = this.connectionInfos.showdbsizecontrols == 'Yes';
    });

    this.localizationService.localizationsLoaded.subscribe((loaded) => {
      console.log("loaded", loaded);
      if (!loaded)
        return;
    });
  }

  ngOnDestroy() {
    this.connectionInfoSub.unsubscribe();
  }
}


