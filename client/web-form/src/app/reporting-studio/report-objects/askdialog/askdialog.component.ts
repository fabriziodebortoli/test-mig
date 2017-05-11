import { ReportingStudioService } from './../../reporting-studio.service';
import { Component, OnInit, Input, OnDestroy, ViewEncapsulation, OnChanges } from '@angular/core';
import { TemplateItem, askGroup, text, check, radio, CommandType, askObj } from './../../reporting-studio.model';

@Component({
  selector: 'rs-askdialog',
  templateUrl: './askdialog.component.html',
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./askdialog.component.scss']
})
export class AskdialogComponent implements /*OnInit,*/ OnDestroy, OnChanges {

  @Input() ask: string;

  public askObject;
  public objects: askGroup[] = [];
  public templates: TemplateItem[] = [];

  constructor(private rsService: ReportingStudioService) {
  }

  /* ngOnInit() {
     this.askObject = JSON.parse(this.ask);
     this.RenderLayout(this.askObject);
   }*/

  ngOnChanges() {
    this.askObject = JSON.parse(this.ask);
    this.RenderLayout(this.askObject);

  }

  ngOnDestroy() {
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
    this.templates.push(new TemplateItem(msg.name, msg, objects));
    this.objects = objects;
    this.rsService.askPage = msg.name;
    return;

  }

  Next() {
    let arrayComp: any[] = [];
    for (let i = 0; i < this.objects.length; i++) {
      let group = this.objects[i];
      for (let j = 0; j < group.entries.length; j++) {
        let component: askObj = group.entries[j];
        let obj = {
          id: component.id,
          value: component.value.toString()
        };
        arrayComp.push(obj);
      }
    }
    let message = {
      commandType: CommandType.ASK,
      message: JSON.stringify(arrayComp),
      page: this.rsService.askPage
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  Prev() {
    if (this.templates.length <= 1) {
      return;
    }
    //this.templates[this.templates.length-1].templateObjects;
    this.templates.pop();
    this.objects = this.templates.pop().templateObjects;
  }

  close() {
    this.rsService.showAsk = false;
    this.ngOnDestroy();
  }

}
