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
import { TemplateItem } from 'app/reporting-studio';
import { LayoutService } from 'app/core/layout.service';


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
 


  // report template objects
  public reportTemplate: any;
  public reportData: any;

  // ask dialog objects
  public askDialogTemplate: any;
  // hotlink possible values
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
    this.eventData.model = { 'Title': { 'value': "..." } };

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
  ngOnDestroy() {
    this.subMessage.unsubscribe();
    this.viewHeightSubscription.unsubscribe();
    if (this.args.params.runAtTbLoader) {
      this.tbLoaderWebSocketService.doCommand(this.rsService.mainCmpId, 'ID_FILE_CLOSE');
    }
  }

  // -----------------------------------------------
  reset() {
    this.rsService.running = false;
    this.askDialogTemplate = 'empty';
    this.hotLinkValues = 'empty';
    this.reportTemplate = 'empty';
    this.reportData = 'empty';
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
          this.eventData.model.Title.value = k.page.report_title;
          this.reportTemplate = k;
          this.RunReport();
          break;
        case CommandType.TEMPLATE:
          this.rsService.showAsk = false;
          this.reportTemplate = k;
          this.GetData();
          break;
        case CommandType.DATA:
          this.rsService.showAsk = false;
          this.reportData = k;
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
  RunReport() {
   this.rsService.running = true;
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
    this.rsService.running = false;
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
  ReRunReport() {

    let message = {
      commandType: CommandType.RERUN,
      message: this.args.nameSpace,
      page: 0
    };

    this.rsService.reset();
    this.reset();
    this.rsService.doSend(JSON.stringify(message));
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
