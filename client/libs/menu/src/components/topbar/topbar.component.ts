import { Component, OnInit, ViewEncapsulation, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { SidenavService } from '@taskbuilder/core';
@Component({
  selector: 'tb-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss']
})
export class TopbarComponent implements OnDestroy {

  subscriptions: Subscription[] = [];
  sidenavPinned: boolean = false;
  sidenavOpened: boolean = false;

  constructor(public sidenavService: SidenavService) {
    this.subscriptions.push(this.sidenavService.sidenavPinned$.subscribe((pinned) => this.sidenavPinned = pinned));
    this.subscriptions.push(this.sidenavService.sidenavOpened$.subscribe((opened) => this.sidenavOpened = opened));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

}
