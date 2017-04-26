import { ReportingStudioService } from './../../reporting-studio.service';
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { TemplateItem } from './../../reporting-studio.model';

@Component({
  selector: 'rs-askdialog',
  templateUrl: './askdialog.component.html',
  styleUrls: ['./askdialog.component.scss']
})
export class AskdialogComponent implements OnInit, OnDestroy {

  @Input() ask: string;

  public askObject;
  public objects: any[] = [];
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
      let element = msg.controls[index];
      objects.push(element);
    }
    this.objects = objects;
    /* let template = this.FindTemplate(msg.page.layout.name);
     if (template !== undefined) {
       this.objects = template.templateObjects;
       this.setDocumentStyle(template.template.page);
       return;
     }
 
    
     this.setDocumentStyle(msg.page);
 
     for (let index = 0; index < msg.page.layout.objects.length; index++) {
       let element = msg.page.layout.objects[index];
       let obj;
       if (element.fieldrect !== undefined) {
         if (element.fieldrect.value_is_image !== undefined && element.fieldrect.value_is_image === true) {
           obj = new graphrect(element.fieldrect);
         }
         else {
           obj = new fieldrect(element.fieldrect);
         }
       }
       else if (element.textrect !== undefined) {
         obj = new textrect(element.textrect);
       }
       else if (element.table !== undefined) {
         obj = new table(element.table);
       }
       else if (element.graphrect !== undefined) {
         obj = new graphrect(element.graphrect);
       }
       else if (element.sqrrect !== undefined) {
         obj = new sqrrect(element.sqrrect);
       }
       else if (element.repeater !== undefined) {
          obj = new repeater(element.repeater);
        }
       objects.push(obj);
     }
 
     this.templates.push(new TemplateItem(msg.page.layout.name, msg, objects));
     this.objects = objects;
     return;*/
  }



  close() {
    this.rsService.showAsk = false;
    this.ngOnDestroy();
  }

}
