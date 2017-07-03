import { LayoutService } from './../../../core/services/layout.service';
import { Component, OnInit, OnDestroy, HostBinding } from '@angular/core';

import { Subscription } from 'rxjs';

@Component({
  selector: 'tb-frame',
  templateUrl: './frame.component.html',
  styleUrls: ['./frame.component.scss']
})
export class FrameComponent implements OnInit, OnDestroy {

  private viewHeightSubscription: Subscription;

  @HostBinding('style.height') viewHeight: Number;

  constructor(private layoutService: LayoutService) { }

  ngOnInit() {
    this.viewHeightSubscription = this.layoutService.getViewHeight().subscribe((viewHeight) => this.viewHeight = viewHeight);
  }

  ngOnDestroy() {
    this.viewHeightSubscription.unsubscribe();
  }

}
