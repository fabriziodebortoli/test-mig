import { Component, OnInit, AfterContentInit, OnDestroy, ViewChild, ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';

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
    public eventData: EventDataService,
    public componentResolver: ComponentFactoryResolver,
    public ciService: ComponentInfoService
  ) {
    super(bo, eventData, ciService);

    this.subscriptions.push(eventData.radarInfos.subscribe((radarInfos: string) => {
      this.radarObj._component.init(radarInfos);
    }));
  }

  ngOnInit() {
    super.ngOnInit();
  }

  ngAfterContentInit() {
    let componentFactory = this.componentResolver.resolveComponentFactory(RadarComponent);
    this.radarObj = this.radarRef.createComponent(componentFactory);
  }

  ngOnDestroy() {
    this.bo.dispose();
    super.ngOnDestroy();
  }

}

