import { WebSocketService } from './../core/websocket.service';
import { UtilsService } from './../core/utils.service';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Subscription } from 'rxjs';
import { CommandType, baseobj, fieldrect, textrect, table, column, graphrect, sqrrect, link } from './reporting-studio.model';
import { DocumentComponent } from '../shared/document.component';
import { ComponentService } from './../core/component.service';
import { EventDataService } from './../core/eventdata.service';
import { ReportingStudioService } from './reporting-studio.service';
import { TemplateItem } from "app/reporting-studio";
import { LayoutService } from "app/core/layout.service";


@Component({
  selector: 'tb-reporting-studio',
  templateUrl: './reporting-studio.component.html',
  styleUrls: ['./reporting-studio.component.scss'],
  providers: [ReportingStudioService, EventDataService],
})
export class ReportingStudioComponent extends DocumentComponent implements OnInit, OnDestroy {

  /*if this component is used standalone, the namespace has to be passed from the outside template,
  otherwise it is passed by the ComponentService creation logic*/
  private subMessage: Subscription;
  private message: any = '';
  public running: boolean = false;

  public layoutStyle: any = {};
  public layoutBackStyle: any = {};
  public objects: baseobj[] = [];
  public templates: TemplateItem[] = [];
  public askDialogTemplate: any;
  public hotLinkValues: any;

  private viewHeightSubscription: Subscription;
  private viewHeight: number;
  private totalPages: number;

  constructor(
    private rsService: ReportingStudioService,
    eventData: EventDataService,
    private cookieService: CookieService,
    private layoutService: LayoutService,
    private componentService: ComponentService,
    private tbLoaderWebSocketService: WebSocketService/*global ws connection used at login level, to communicatewith tbloader */) {
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
      message: "",
      page: this.rsService.pageNum
    };
    this.rsService.doSend(JSON.stringify(message));

    this.viewHeightSubscription = this.layoutService.getViewHeight().subscribe((viewHeight) => this.viewHeight = viewHeight);
  }

  // -----------------------------------------------
  ngOnDestroy() {
    this.subMessage.unsubscribe();
    this.viewHeightSubscription.unsubscribe();
    if (this.args.params.runAtTbLoader) {
      this.tbLoaderWebSocketService.doCommand(this.rsService.mainCmpId, 'ID_FILE_CLOSE');
    }
  }

  // -----------------------------------------------
  onMessage(message: any) {
    //elaborate
    try {
      let msg = JSON.parse(message);
      let k = msg.message !== "" ? JSON.parse(msg.message) : undefined;
      switch (msg.commandType) {
        case CommandType.ASK:
          this.askDialogTemplate = message;
          this.rsService.showAsk = true;
          break;
        case CommandType.HOTLINK:
          this.hotLinkValues = msg;
          break;
        case CommandType.UPDATEASK:
          this.askDialogTemplate = message;
          break;
        case CommandType.PREVASK:
          this.askDialogTemplate = message;
          break;
        case CommandType.NAMESPACE: break;
        case CommandType.STOP: break;
        case CommandType.INITTEMPLATE:
          this.RenderLayout(k);
          this.RunReport();
          break;
        case CommandType.TEMPLATE:
          this.rsService.showAsk = false;
          this.RenderLayout(k);
          this.GetData();
          break;
        case CommandType.DATA:
          this.rsService.showAsk = false;
          this.UpdateData(k);
          break;
        case CommandType.RUNREPORT:
          const params = { /*xmlArgs: encodeURIComponent(k.arguments),*/ xargs: encodeURIComponent(k.args), runAtTbLoader: false };
          this.componentService.createReportComponent(k.ns, true, params);
          break;
        case CommandType.ENDREPORT:
          this.totalPages = k.totalPages;
          break;
        case CommandType.NONE:
          break;
        case CommandType.WRONG:
          break;
      }
      //TODO when report finishes execution, send result to tbloader server report (if any)
      //if (this.args.params.runAtTbLoader) {
      // this.tbLoaderWebSocketService.setReportResult(this.rsService.mainCmpId, {});
      //}
      //this.message = msg;//.message;
    } catch (err) {
      this.message = 'Error Occured';
    }
  }

  // -----------------------------------------------
  rsInitStateMachine() {
    let p: string = '';
    let p2: string = '';
    if (this.args.params) {
      if (this.args.params.xargs != null) {
        p = JSON.stringify(this.args.params.xargs);
        p2 = decodeURIComponent(p);
      }
      else p2 = this.args.params.xmlArgs ? decodeURIComponent(this.args.params.xmlArgs) : JSON.stringify(this.args.params);
    }
    let message = {
      commandType: CommandType.NAMESPACE,
      nameSpace: this.args.nameSpace,
      parameters: p2,
      authtoken: this.cookieService.get('authtoken')
    };
    this.rsService.doSendSync(JSON.stringify(message));
  }


  // -----------------------------------------------
  RunReport() {
    this.running = true;
    let message = {
      commandType: CommandType.ASK,
      message: '',
      page: ''
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  GetData() {
    let message = {
      commandType: CommandType.DATA,
      message: this.args.params.xmlArgs,
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
    if (this.rsService.pageNum < this.totalPages) {
      this.rsService.pageNum++;
    }
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
  FirstPage() {
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: 1
    };

    this.rsService.pageNum = message.page;
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  LastPage() {
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: this.totalPages
    };

    this.rsService.pageNum = message.page;
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  RenderLayout(msg: any) {

    let template = this.FindTemplate(msg.page.layout.name);
    if (template !== undefined) {
      this.objects = template.templateObjects;
      this.setDocumentStyle(template.template.page);
      return;
    }

    let objects = [];
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
      /* else if (element.repeater !== undefined) {
         obj = new repeater(element.repeater);
       }*/
      objects.push(obj);
    }

    this.templates.push(new TemplateItem(msg.page.layout.name, msg, objects));
    this.objects = objects;
    return;
  }

  // -----------------------------------------------
  UpdateData(msg: any) {

    if (this.rsService.pageNum !== msg.page.page_number) {
      return;
    }
    let id: string;
    let value: any;
    for (let index = 0; index < msg.page.layout.objects.length; index++) {
      let element = msg.page.layout.objects[index];
      try {
        if (element.fieldrect !== undefined) {
          id = element.fieldrect.baserect.baseobj.id;
          value = element.fieldrect.value;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
          }
          if (obj.link !== undefined) {
            obj.link = new link(element.fieldrect.link);
          }
          obj.value = value;
        }
        else if (element.textrect !== undefined) {
          id = element.textrect.baserect.baseobj.id;
          value = element.textrect.value;
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
          }
          obj.value = value;
        }
      } catch (a) {
        let k = a;
      }
    }
  }

  // -----------------------------------------------
  setDocumentStyle(layout: any) {

    this.layoutStyle = {
      'width': (layout.pageinfo.width) + 'mm',
      'height': (layout.pageinfo.length) + 'mm',
      'background-color': 'white',
      'border': '1px solid #ccc',
      'margin': '5px auto',
      'position': 'relative'
    }
    this.layoutBackStyle = {
      'width': '100%',
      'position': 'relative',
      'overflow': 'scroll',
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
  private FindTemplate(name: string): TemplateItem {
    for (let index = 0; index < this.templates.length; index++) {
      if (this.templates[index].templateName === name) {
        return this.templates[index];
      }
    }
    return undefined;
  }
}


@Component({
  template: ''
})
export class ReportingStudioFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver, private activatedRoute: ActivatedRoute) {
    this.activatedRoute.params.subscribe((params: Params) => {
      let ns = params['ns'];
      let pars = params['params'];
      pars = pars ? JSON.parse(pars) : {};

      componentService.createComponent(
        ReportingStudioComponent,
        resolver,
        { 'nameSpace': ns, 'params': pars }
      );
    });
  }
}
