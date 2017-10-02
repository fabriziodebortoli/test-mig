import { ComponentInfoService } from './../../core/services/component-info.service';
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
export abstract class BOCommonComponent extends DocumentComponent implements OnInit, OnDestroy {
  subscriptions: Subscription[] = [];

  constructor(document: BOService,
    eventData: EventDataService,
    ciService: ComponentInfoService) {
    super(document, eventData, ciService);

    this.culture = this.ciService.globalInfoService.culture.value;
    let me = this;
    this.subscriptions.push(document.windowStrings.subscribe((args: any) => {
      if (me.cmpId === args.id) {
        me.translations = args.strings;
        let jItem = { translations: me.translations, installationVersion: this.installationVersion };
        localStorage.setItem(this.dictionaryId, JSON.stringify(jItem));
      }
    }));
  }


  readTranslationsFromServer() {
    let s = this.document as BOService;
    s.getWindowStrings(this.cmpId, this.culture);
  }
  ngOnInit() {
    const ci = this.ciService.componentInfo;
    this.dictionaryId = ci.app.toLowerCase() + '/' + ci.mod.toLowerCase() + '/' + ci.name + '/' + this.culture;
    super.ngOnInit();
  }
  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
}
@Component({
  selector: 'tb-bo-base',
  template: '',
  styles: []
})
export class BOComponent extends BOCommonComponent implements OnInit, AfterContentInit, OnDestroy {


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

@Component({
  selector: 'tb-bo-slave',
  template: '',
  styles: []
})
export abstract class BOSlaveComponent extends BOCommonComponent implements OnInit, OnDestroy {

  constructor(eventData: EventDataService, ciService: ComponentInfoService) {
    super(null, eventData, ciService);
  }

  ngOnInit() {
    super.ngOnInit();
  }
  ngOnDestroy() {

    super.ngOnDestroy();
  }

}