import { Component, OnInit, OnDestroy, ViewChild, ViewContainerRef, ComponentFactoryResolver, AfterContentInit } from '@angular/core';

import { ControlTypes } from "../models/control-types.enum";

import { EventDataService } from './../../core/services/eventdata.service';
import { BOService } from './../../core/services/bo.service';

import { RadarComponent } from './radar/radar.component';
import { DocumentComponent } from './document.component';
import { Subscription } from "rxjs/Subscription";

@Component({
  selector: 'tb-bo',
  template: '',
  styles: []
})
export abstract class BOComponent extends DocumentComponent implements OnInit, AfterContentInit, OnDestroy {

  subscriptions: Subscription[] = [];

  @ViewChild("radar", { read: ViewContainerRef }) target: ViewContainerRef;
  private radarRef; // Radar template reference
  private radarObj: RadarComponent; // Radar obj reference

  controlTypeModel = ControlTypes;
  constructor(public bo: BOService, eventData: EventDataService, private componentResolver: ComponentFactoryResolver) {
    super(bo, eventData);

    this.subscriptions.push(eventData.radarInfos.subscribe(radarInfos => {
      console.log('radarInfos', radarInfos);
      this.radarObj.init(radarInfos)
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
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

}
