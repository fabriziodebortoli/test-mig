import { Component, OnInit, AfterContentInit, OnDestroy, ViewChild, ViewContainerRef, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { ControlTypes } from '../../models/control-types.enum';

import { BOCommonComponent } from './bo-common.component';
import { RadarComponent } from './../radar/radar.component';

import { ComponentInfoService } from './../../../core/services/component-info.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { BOService } from './../../../core/services/bo.service';

@Component({
  selector: 'tb-bo-base',
  template: '',
  styles: []
})
export class BOComponent extends BOCommonComponent implements OnInit, AfterContentInit, OnDestroy {

  subscriptions: Subscription[] = [];

  @ViewChild("radar", { read: ViewContainerRef }) radarRef: ViewContainerRef;// Radar template reference
  public radarObj//: RadarComponent; // Radar obj reference

  controlTypeModel = ControlTypes;

  constructor(
    public bo: BOService,
    eventData: EventDataService,
    ciService: ComponentInfoService,
    changeDetectorRef: ChangeDetectorRef,
    public componentResolver: ComponentFactoryResolver
  ) {
    super(bo, eventData, ciService, changeDetectorRef);
  }

  ngOnInit() {
    super.ngOnInit();
  }

  ngAfterContentInit() {
    let componentFactory = this.componentResolver.resolveComponentFactory(RadarComponent);
    if (this.radarRef)
      this.radarObj = this.radarRef.createComponent(componentFactory);
  }

  ngOnDestroy() {
    this.bo.dispose();
    super.ngOnDestroy();
  }

}

