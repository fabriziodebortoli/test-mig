import { ComponentInfoService, ComponentInfo } from './../models/component-info.model';
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
  translations = [];
  subscriptions: Subscription[] = [];
  culture = 'it-IT';
  installationVersion = '';
  constructor(document: BOService,
    eventData: EventDataService,
    ciService: ComponentInfoService) {
    super(document, eventData, ciService);

    let me = this;
    this.subscriptions.push(document.windowStrings.subscribe((args: any) => {
      if (me.cmpId === args.id) {
        me.translations = args.strings;
        let jItem = { translations: me.translations, installationVersion: this.installationVersion };
        localStorage.setItem(this.getDictionaryID(this.ciService.componentInfo), JSON.stringify(jItem));
      }
    }));
  }
  getDictionaryID(ci: ComponentInfo) {
    return ci.app.toLowerCase() + '/' + ci.mod.toLowerCase() + '/' + ci.name + '/' + this.culture;
  }
  l(baseText: string) {
    let target = baseText;
    this.translations.some(t => {
      if (t.base == baseText) {
        target = t.target;
        return true;
      }
      return false;
    });
    return target;
  }
  ngOnInit() {
    let sub = this.ciService.globalInfoService.getProductInfo().subscribe((productInfo: any) => {
      this.installationVersion = productInfo.installationVersion;
      if (sub)
        sub.unsubscribe();
      this.readTranslations();
    });

    super.ngOnInit();
  }
  private readTranslations() {
    let item = localStorage.getItem(this.getDictionaryID(this.ciService.componentInfo));
    let found = false;
    if (item) {
      try {
        let jItem = JSON.parse(item);

        if (jItem.installationVersion === this.installationVersion) {
          this.translations = jItem.translations;
          found = true;
        }
      }
      catch (ex) {
        console.log(ex);
      }
    }
    if (!found) {
      let s = this.document as BOService;
      s.getWindowStrings(this.cmpId, this.culture);
    }
  }
  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
}
export abstract class BOComponent extends BOCommonComponent implements OnInit, AfterContentInit, OnDestroy {


  @ViewChild("radar", { read: ViewContainerRef }) radarRef: ViewContainerRef;// Radar template reference
  private radarObj//: RadarComponent; // Radar obj reference

  controlTypeModel = ControlTypes;
  constructor(public bo: BOService,
    eventData: EventDataService,
    private componentResolver: ComponentFactoryResolver,
    ciService: ComponentInfoService) {
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