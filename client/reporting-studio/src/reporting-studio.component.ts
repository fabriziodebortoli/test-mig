import { ReportLayoutComponent } from './report-objects/layout/layout.component';
import { WebSocketService, HttpService } from '@taskbuilder/core';
import { UtilsService } from '@taskbuilder/core';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Subscription } from 'rxjs';
import { CommandType, baseobj, fieldrect, textrect, table, column, graphrect, sqrrect, link, PdfType, SvgType, PngType } from './models';
import { DocumentComponent } from '@taskbuilder/core';
import { ComponentService } from '@taskbuilder/core';
import { EventDataService } from '@taskbuilder/core';
import { ReportingStudioService } from './reporting-studio.service';
import { Snapshot } from './report-objects/snapshotdialog/snapshot';

import { Image, Surface, Path, Text, Group, drawDOM, DrawOptions, exportPDF } from '@progress/kendo-drawing';
import { saveAs } from '@progress/kendo-file-saver';

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

  //Export excel
  public data: any[];

  public curPageNum: number;

  constructor(
    private rsService: ReportingStudioService,
    eventData: EventDataService,
    private cookieService: CookieService,
    private httpServ: HttpService,

    private componentService: ComponentService,
    private tbLoaderWebSocketService: WebSocketService/*global ws connection used at login level, to communicatewith tbloader */) {
    super(rsService, eventData, null);
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

    this.rsService.rsExportPdf.subscribe(() => this.startSavePDF());
    this.rsService.rsExportExcel.subscribe(() => this.startSaveExcel());
    this.rsService.rsExportDocx.subscribe(() => this.startSaveDocx());
    this.rsService.eventNextPage.subscribe(() => this.NextPage());
    this.rsService.eventFirstPage.subscribe(() => this.FirstPage());
    this.rsService.eventCurrentPage.subscribe(() => this.CurrentPage());
    this.rsService.eventSnapshot.subscribe(() => this.Snapshot());
    this.rsService.runSnapshot.subscribe(() => this.RunSnapshot());
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

    if (this.args.params.runAtTbLoader) {
      this.tbLoaderWebSocketService.closeServerComponent(this.rsService.mainCmpId);
    }
  }

  // -----------------------------------------------
  reset() {
    this.rsService.running = false;
    this.askDialogTemplate = 'empty';

    this.reportTemplate = 'empty';
    this.reportData = 'empty';
  }

  // -----------------------------------------------
  onMessage(message: any) {
    //elaborate
    try {
      let msg = JSON.parse(message);
      let k = msg.message !== "" ? msg.message : undefined;
      switch (msg.commandType) {
        case CommandType.ASK:
          this.askDialogTemplate = message;
          this.rsService.showAsk = true;
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
          this.rsService.titleReport = k.page.report_title;
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
          this.data = k;
          this.curPageNum = k.page.page_number;
          break;
        case CommandType.RUNREPORT:
          const params = { /*xmlArgs: encodeURIComponent(k.arguments),*/ xargs: encodeURIComponent(k.args), runAtTbLoader: false };
          this.componentService.createReportComponent(k.ns, true, params);
          break;
        case CommandType.ENDREPORT:
          this.rsService.totalPages = k.totalPages;
          this.rsService.runEnabled = true;
          break;
        case CommandType.NONE:
          break;
        case CommandType.WRONG:
          break;
        case CommandType.EXPORTEXCEL:
          if (k == "Errore") {
            window.alert("Errore: non ci sono dati da esportare in Excel");
            break;
          }
          this.getExcelData(k + ".xlsx");
          break;
        case CommandType.EXPORTDOCX:
          this.getDocxData(k + ".docx");
          break;
        case CommandType.SNAPSHOT:
          this.FirstPage();
          break;
        case CommandType.ACTIVESNAPSHOT:
          this.CreateTableSnapshots(k);
          break;
      }
      //TODO when report finishes execution, send result to tbloader server report (if any)
      //if (this.args.params.runAtTbLoader) {
      // this.tbLoaderWebSocketService.setReportResult(this.rsService.mainCmpId, {});
      //}
      //this.message = msg;//.message;
    } catch (err) {
      console.log(err);
      this.message = 'Error Occured';
    }
  }

  // -----------------------------------------------
  RunReport() {
    this.rsService.running = true;
    this.rsService.runEnabled = false;
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
    if (this.rsService.pageNum < this.rsService.totalPages) {
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
      page: this.rsService.totalPages
    };

    this.rsService.pageNum = message.page;
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  ReRunReport() {
    this.rsService.reset();
    this.reset();

    this.rsInitStateMachine();

    let message = {
      commandType: CommandType.RERUN
    };
    this.rsService.doSend(JSON.stringify(message));
  }


  // -----------------------------------------------
  CurrentPage() {
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: this.curPageNum
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  PageNumber() {
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: this.rsService.firstPageExport
    };

    this.rsService.pageNum = message.page;
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  Snapshot() {
    //il flag user-allUser è passato insieme al numeroPagina
    let message = {
      commandType: CommandType.SNAPSHOT,
      message: this.args.nameSpace,
      page: 1 + "," + this.rsService.nameSnap + "," + this.rsService.user
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  CreateTableSnapshots(k: Snapshot[]) {
    this.rsService.snapshots = k;
  }

  // -----------------------------------------------
  RunSnapshot() {
    this.rsService.snapshot = false;
    let message = {
      commandType: CommandType.RUNSNAPSHOT,
      message: this.args.nameSpace,
      page: 1 + "," + this.rsService.dateSnap + "_" + this.rsService.nameSnap + "," + this.rsService.user
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  //--------------------------------------------------
  public startSaveSVG() {
    this.rsService.svgState = SvgType.SVG;
    this.CurrentPage();
  }

  //--------------------------------------------------
  public startSavePNG() {
    this.rsService.pngState = PngType.PNG;
    this.CurrentPage();
  }

  //--------------------------------------------------
  setExportFile(type: string) {
    if (type == this.rsService.pdf)
      this.rsService.exportpdf = true;
    if (type == this.rsService.excel)
      this.rsService.exportexcel = true;
    if (type == this.rsService.docx)
      this.rsService.exportdocx = true;
    this.rsService.exportfile = true
  }

  //--------------------------------------------------
  public startSavePDF() {
    this.rsService.pdfState = PdfType.PDF;
    this.PageNumber();
  }

  //--------------------------------------------------
  public startSaveExcel() {
    let message = {
      commandType: CommandType.EXPORTEXCEL,
      message: this.args.nameSpace,
      page: this.rsService.firstPageExport + "," + this.rsService.lastPageExport
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  //--------------------------------------------------
  public startSaveDocx() {
    let message = {
      commandType: CommandType.EXPORTDOCX,
      message: this.args.nameSpace,
      page: this.curPageNum
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  //--------------------------------------------------
  public startSnapshot() {
    this.rsService.snapshot = true;
    let message = {
      commandType: CommandType.ACTIVESNAPSHOT,
      message: this.args.nameSpace,
      page: 1
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  //--------------------------------------------------
  getExcelData(filename: string) {
    var iframeHTML = document.getElementById('iframe') as HTMLFrameElement;
    var s = this.httpServ.getReportServiceUrl() + 'excel/' + filename;
    iframeHTML.src = s;
  }

  //--------------------------------------------------
  getDocxData(filename: string) {
    var iframeHTML = document.getElementById('iframe') as HTMLFrameElement;
    var s = this.httpServ.getReportServiceUrl() + 'docx/' + filename;
    iframeHTML.src = s;
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

