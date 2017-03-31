
import { CookieService } from 'angular2-cookie/services/cookies.service';

import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Subscription } from 'rxjs';

import { CommandType, baseobj, fieldrect, textrect, table, column, graphrect, sqrrect, repeater, column_total } from './reporting-studio.model';

import { DocumentComponent } from '../shared/document.component';

import { ComponentService } from './../core/component.service';
import { EventDataService } from './../core/eventdata.service';
import { ReportingStudioService } from './reporting-studio.service';


@Component({
  selector: 'app-reporting-studio',
  templateUrl: './reporting-studio.component.html',
  styleUrls: ['./reporting-studio.component.scss'],
  providers: [ReportingStudioService, EventDataService],
})
export class ReportingStudioComponent extends DocumentComponent implements OnInit, OnDestroy {

  /*if this component is used standalone, the namespace has to be passed from the outside template,
  otherwise it is passed by the ComponentService creation logic*/
  private subMessage: Subscription;
  private message: any = '';
  private running: boolean = false;
  public layoutStyle: any = {};
  public layoutBackStyle: any = {};
  public objects: baseobj[] = [];
  public templates: TemplateItem[] = [];

  constructor(private rsService: ReportingStudioService, eventData: EventDataService, private cookieService: CookieService) {
    super(rsService, eventData);
  }

  // -----------------------------------------------
  ngOnInit() {
    super.ngOnInit();
    this.eventData.model = { 'Title': { 'value': this.args.nameSpace } };

    this.subMessage = this.rsService.message.subscribe(received => {
      this.onMessage(received);

    });

    this.rsInitStateMachine();

    let message = {
      commandType: CommandType.INITTEMPLATE,
      message: '',
      page: this.rsService.pageNum
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  ngOnDestroy() {
    this.subMessage.unsubscribe();
  }

  // -----------------------------------------------
  onMessage(message: any) {
    //elaborate
    try {
      let msg = JSON.parse(message);
      let k = JSON.parse(msg.message);
      switch (msg.commandType) {
        case CommandType.ASK: break;
        case CommandType.OK: break;
        case CommandType.STOP: break;
        case CommandType.INITTEMPLATE:
          this.templates.push(new TemplateItem(k.page.layout.name, k));
          this.RenderLayout(k);
          break;
        case CommandType.TEMPLATE:
          let template = this.FindTemplate(k.page.layout.name);
          if (template === undefined) {
            this.templates.push(new TemplateItem(k.page.layout.name, k));
            template = k;
          }
          this.RenderLayout(template);
          this.GetData();
          break;
        case CommandType.DATA:
          this.UpdateData(k);
          break;
      }

      //this.message = msg;//.message;
    } catch (err) {
      this.message = 'Error Occured';
    }
  }

  // -----------------------------------------------
  rsInitStateMachine() {

    let message = {
      commandType: CommandType.NAMESPACE,
      nameSpace: this.args.nameSpace,
      authtoken: this.cookieService.get('authtoken')
    };

    this.rsService.doSendSync(JSON.stringify(message));

  }

  // -----------------------------------------------
  RunReport() {
    this.running = true;

    //ASK
    let message = {
      commandType: CommandType.DATA,
      message: this.args.nameSpace,
      page: 0
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  GetData() {

    let message = {
      commandType: CommandType.DATA,
      message: this.args.nameSpace,
      page: 0
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  StopReport() {
    this.running = false;

    let message = {
      commandType: CommandType.STOP,
      message: this.args.nameSpace,
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  NextPage() {
    this.rsService.pageNum++;
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: this.rsService.pageNum
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  PrevPage() {
    if (this.rsService.pageNum > 1) {
      this.rsService.pageNum--;
    }
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: this.rsService.pageNum
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  RenderLayout(msg: any) {
    this.objects = [];
    if (this.rsService.pageNum !== msg.page.page_number) { return; }
    this.rsService.currLayout = msg.page.layout.name;
    this.setDocumentStyle(msg.page);
    for (let index = 0; index < msg.page.layout.objects.length; index++) {
      let element = msg.page.layout.objects[index];
      let obj;
      if (element.fieldrect !== undefined) {
        obj = new fieldrect(element.fieldrect);
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
      /* else if (element.repeater !== undefined) {
         obj = new repeater(element.fieldrect);
       }*/
      this.objects.push(obj);
    }
  }

  // -----------------------------------------------
  UpdateData(msg: any) {

    if (this.rsService.pageNum != msg.page.page_number) { return; }
    let id: string;
    let value: any;
    for (let index = 0; index < msg.page.layout.objects.length; index++) {
      let element = msg.page.layout.objects[index];
      if (element.fieldrect !== undefined) {
        id = element.fieldrect.baserect.baseobj.id;
        value = element.fieldrect.value ? element.fieldrect.value : '[empty]' + id;
        let obj = this.FindObj(id);
        if (obj === undefined) {
          continue;
        }
        obj.value = value;
      }
      else if (element.textrect !== undefined) {
        id = element.textrect.baserect.baseobj.id;
        value = element.textrect.value ? element.textrect.value : '[empty]' + id;
        let obj = this.FindObj(id);
        if (obj === undefined) {
          continue;
        }
        obj.value = value;
      }
      else if (element.table !== undefined) {
        id = element.table.baseobj.id;
        value = element.table.rows;
        let obj = this.FindObj(id);
        if (obj === undefined) {
          continue;
        }

        let columns = element.table.columns;

        for (let i = 0; i < obj.columns.length; i++) {
          let target: column = obj.columns[i];
          let source: column = columns[i];
          if (target.id !== source.id) {
            console.log('id don\'t match');
            continue;
          }
          if (source.hidden !== undefined) {
            target.hidden = source.hidden;
          }
          if (source.total !== undefined) {
            target.total = new column_total(source.total);
          }
        }
        obj.value = value;
      }
    }
  }

  // -----------------------------------------------
  setDocumentStyle(layout: any) {

    this.layoutStyle = {
      'width': layout.pageinfo.width + 'px',
      'height': layout.pageinfo.length + 'px',
      'background-color': 'white',
      'position': 'relative'
    }
    this.layoutBackStyle = {
      'width': '100%',
      'height': '100%',
      'background-color': 'gray',
      'position': 'relative'
    }
  }

  // -----------------------------------------------
  private FindObj(id: string): any {
    for (let key in this.objects) {
      if (this.objects.hasOwnProperty(key)) {
        let element = this.objects[key];
        if (element.id === id) {
          return element;
        }
      }
    }
    return undefined;
  }

  // -----------------------------------------------
  private FindTemplate(name: string): any {
    for (let index = 0; index < this.templates.length; index++) {
      if (this.templates[index].templateName === name) {
        return this.templates[index].template;
      }
      return undefined;
    }
  }
}



export class TemplateItem {
  public templateName: string;
  public template: any;

  constructor(tName: string, tObj: any) {
    this.templateName = tName;
    this.template = tObj;
  }
}

@Component({
  template: ''
})
export class ReportingStudioFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver, private activatedRoute: ActivatedRoute) {
    this.activatedRoute.params.subscribe((params: Params) => {
      let ns = params['ns'];
      componentService.createComponent(ReportingStudioComponent, resolver, { 'nameSpace': ns });
    });
  }
}