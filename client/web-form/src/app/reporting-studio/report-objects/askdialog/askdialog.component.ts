import { ReportingStudioService } from './../../reporting-studio.service';
import { Component, OnInit, Input, OnDestroy, ViewEncapsulation } from '@angular/core';
import { TemplateItem, askGroup, text, check, radio } from './../../reporting-studio.model';

@Component({
  selector: 'rs-askdialog',
  templateUrl: './askdialog.component.html',
   encapsulation: ViewEncapsulation.None,
  styleUrls: ['./askdialog.component.scss']
})
export class AskdialogComponent implements OnInit, OnDestroy {

  @Input() ask: string;

  public askObject;
  public objects: askGroup[] = [];
  public templates: TemplateItem[] = [];

  constructor(private rsService: ReportingStudioService) {
  }

  ngOnInit() {
    this.askObject = JSON.parse(this.ask);
    this.RenderLayout(this.askObject);
  }

  ngOnDestroy() {
    //Called once, before the instance is destroyed.
    //Add 'implements OnDestroy' to the class.
  }

  RenderLayout(msg: any) {
    let objects = [];
    for (let index = 0; index < msg.controls.length; index++) {
      let m = msg.controls[index];
      let element: askGroup = new askGroup(msg.controls[index]);

      objects.push(element);
    }
    this.objects = objects;
    return;

  }

  Next() {
  }

  Prev() {
  }

  close() {
    this.rsService.showAsk = false;
    this.ngOnDestroy();
  }

}
