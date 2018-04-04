import { AskdialogService } from './askdialog.service';
import { Subscription } from '../../rxjs.imports';
import { ReportingStudioService } from './../../reporting-studio.service';
import { Component, Input, OnDestroy, ViewEncapsulation, OnChanges, SimpleChange, EventEmitter, Output } from '@angular/core';

import { text } from './../../models/text.model';
import { check } from './../../models/check.model';
import { radio } from './../../models/radio.model';
import { askObj } from './../../models/ask-obj.model';
import { hotlink } from './../../models/hotlink.model';
import { AskObjectType } from './../../models/ask-object-type.model';
import { TemplateItem } from '../../models/template-item.model';
import { askGroup } from '../../models/ask-group.model';
import { CommandType } from '../../models/command-type.model';

@Component({
  selector: 'rs-askdialog',
  templateUrl: './askdialog.component.html',
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./askdialog.component.scss'],
  providers: [AskdialogService]
})
export class AskdialogComponent implements OnDestroy, OnChanges {

  @Input() ask: string;

  public askObject;
  public commType: CommandType;
  public objects: askGroup[] = [];
  public templates: TemplateItem[] = [];
  subscriptions: Subscription[] = [];

  constructor(public rsService: ReportingStudioService, public adService: AskdialogService) {
    this.subscriptions.push(adService.askChanged.subscribe(() => {
      this.updateAsk();
    }));
  }


  ngOnChanges(changes: { [propKey: string]: SimpleChange }) {
    if (changes.ask !== undefined) {
      if (changes.ask.currentValue === 'empty') {
        this.ask = undefined;
        this.objects = undefined;
        this.templates = undefined;
      }
      else {
        let msg = JSON.parse(this.ask);
        this.commType = msg.commandType;
        this.askObject = msg.message;
        this.RenderLayout(this.askObject);
      }
    }
  }


  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    //Called once, before the instance is destroyed.
    //Add 'implements OnDestroy' to the class.
  }

  RenderLayout(msg: any) {
    let objects = [];
    let error: string = undefined;
    for (let index = 0; index < msg.controls.length; index++) {
      try {
        let m = msg.controls[index];
        let element: askGroup = new askGroup(msg.controls[index]);
        objects.push(element);
      }
      catch (err) {
        error = 'Error Occured' + err.ToString();
      }
    }
    if (this.commType == CommandType.UPDATEASK) {
      this.templates.pop();
    }
    this.templates.push(new TemplateItem(msg.name, msg, objects));
    this.objects = objects;
    return;

  }

  SendAsk(ct: CommandType) {
    let arrayComp: any[] = [];
    for (let i = 0; i < this.objects.length; i++) {
      let group = this.objects[i];
      for (let j = 0; j < group.entries.length; j++) {
        let component: askObj = group.entries[j];
        let obj;
        obj = {
          name: component.name,
          value: component.value.toString()
        };
        arrayComp.push(obj);
      }
    }
    let message = {
      commandType: ct,
      message: JSON.stringify(arrayComp),
      page: this.askObject.name
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  Next() {
    this.SendAsk(CommandType.ASK);
  }

  Prev() {
    if (this.templates.length <= 1) {
      return;
    }
    this.templates.pop();
    this.objects = this.templates[this.templates.length - 1].templateObjects;
    this.askObject = this.templates[this.templates.length - 1].template;
    let message = {
      commandType: CommandType.PREVASK,
      message: '',
      page: this.askObject.name
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  close() {
    this.rsService.showAsk = false;
    this.rsService.running = false;
    this.rsService.runEnabled = true;
    let message = {
      commandType: CommandType.ABORTASK,
      message: '',
      page: this.askObject.name
    };
    this.rsService.doSend(JSON.stringify(message));
    this.ngOnDestroy();
  }

  updateAsk() {
    this.SendAsk(CommandType.UPDATEASK);
  }

}
