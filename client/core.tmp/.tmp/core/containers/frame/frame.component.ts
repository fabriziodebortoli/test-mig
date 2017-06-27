import { Component, OnInit, OnDestroy, HostBinding } from '@angular/core';

import { Subscription } from 'rxjs';

import { LayoutService } from './../../services/layout.service';

@Component({
  selector: 'tb-frame',
  template: "<div [style.height.px]=\"viewHeight\" class=\"tb-frame\"> <ng-content></ng-content> </div>",
  styles: [".tb-frame { display: flex; flex-direction: column; } "]
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
