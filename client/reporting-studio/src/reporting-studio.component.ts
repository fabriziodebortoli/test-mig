import { HttpServiceRs } from './services/rs-httpservice';
import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ElementRef, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';

import { WebSocketService, InfoService, DocumentComponent, ComponentService, EventDataService, UtilsService } from '@taskbuilder/core';

import { Image, Surface, Path, Text, Group, drawDOM, DrawOptions, exportPDF } from '@progress/kendo-drawing';
import { saveAs } from '@progress/kendo-file-saver';

import { baseobj } from './models/baseobj.model';
import { fieldrect } from './models/fieldrect.model';
import { textrect } from './models/textrect.model';
import { table } from './models/table.model';
import { column } from './models/column.model';
import { graphrect } from './models/graphrect.model';
import { sqrrect } from './models/sqrrect.model';
import { link } from './models/link.model';
import { CommandType } from './models/command-type.model';
import { PdfType, SvgType, PngType } from './models/export-type.model';

import { Subscription } from './rxjs.imports';

import { RsExportService } from './rs-export.service';
import { ReportLayoutComponent } from './report-objects/layout/layout.component';
import { ReportingStudioService } from './reporting-studio.service';
import { Snapshot } from './report-objects/snapshotdialog/snapshot';

@Component({
  selector: 'tb-reporting-studio',
  templateUrl: './reporting-studio.component.html',
  styleUrls: ['./reporting-studio.component.scss'],
  providers: [ReportingStudioService, RsExportService, EventDataService, HttpServiceRs]
})

export class ReportingStudioComponent extends DocumentComponent implements OnInit, OnDestroy {

  /*if this component is used standalone, the namespace has to be passed from the outside template,
  otherwise it is passed by the ComponentService creation logic*/
  public subMessage: Subscription;
  public message: any = '';

  // report template objects
  public reportTemplate: any;
  public reportData: any;

  // ask dialog objects
  public askDialogTemplate: any;

  //Export excel
  public data: any[];

  public curPageNum: number;
  public runningReport: boolean = false;

  public id: string;

  constructor(
    public rsService: ReportingStudioService,
    public rsExportService: RsExportService,
    public httpServiceRs : HttpServiceRs,
    eventData: EventDataService,
    changeDetectorRef: ChangeDetectorRef,
    public infoService: InfoService,

    public componentService: ComponentService,
    public tbLoaderWebSocketService: WebSocketService/*global ws connection used at login level, to communicatewith tbloader */) {
    super(rsService, eventData, null, changeDetectorRef);

    this.id = this.rsService.generateId();
    this.rsExportService.layoutId = this.id
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

    this.rsExportService.rsExportPdf.subscribe(() => this.startSavePDF());
    this.rsExportService.rsExportExcel.subscribe(() => this.startSaveExcel());
    this.rsExportService.rsExportDocx.subscribe(() => this.startSaveDocx());
    this.rsExportService.eventNextPage.subscribe(() => this.nextPage());
    this.rsExportService.eventFirstPage.subscribe(() => this.firstPage());
    this.rsExportService.eventCurrentPage.subscribe(() => this.currentPage());
    this.rsExportService.eventSnapshot.subscribe(() => this.snapshot());
    this.rsExportService.runSnapshot.subscribe(() => this.runSnapshot());
    this.rsExportService.eventPageNumber.subscribe(() => this.pageNumber());
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
    let message: any = {
      commandType: CommandType.NAMESPACE,
      nameSpace: this.args.nameSpace,
      parameters: p2,
      authtoken: sessionStorage.getItem('authtoken'),
      tbLoaderName: this.infoService.getTbLoaderInfo().name
    };

    if (this.args.params.runAtTbLoader) {
      message.componentId = this.cmpId;
    }

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
          this.rsExportService.titleReport = k.page.report_title;
          this.reportTemplate = k;
          this.runReport();
          break;
        case CommandType.TEMPLATE:
          this.rsService.showAsk = false;
          this.reportTemplate = k;
          this.getData();
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
          this.rsExportService.totalPages = k.totalPages;
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
          this.runningReport = true;
          this.rsExportService.totalPages = parseInt(msg.page);
          this.firstPage();
          break;
        case CommandType.ACTIVESNAPSHOT:
          this.createTableSnapshots(k);
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
  runReport() {
    this.rsService.runEnabled = false;
    let message = {
      commandType: CommandType.ASK,
      message: '',
      page: ''
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  getData() {
    let message = {
      commandType: CommandType.DATA,
      message: this.args.params.xmlArgs,
      page: 0
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  stopReport() {
    let message = {
      commandType: CommandType.STOP,
      message: this.args.nameSpace,
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  nextPage() {
    if (this.rsService.pageNum < this.rsExportService.totalPages) {
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
  prevPage() {
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
  firstPage() {
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: 1
    };

    this.rsService.pageNum = message.page;
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  lastPage() {
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: this.rsExportService.totalPages
    };

    this.rsService.pageNum = message.page;
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  reRunReport() {
    this.rsService.reset();
    this.reset();

    this.rsInitStateMachine();

    let message = {
      commandType: CommandType.RERUN
    };
    this.rsService.doSend(JSON.stringify(message));
  }


  // -----------------------------------------------
  currentPage() {
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: this.curPageNum
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  pageNumber() {
    let message = {
      commandType: CommandType.TEMPLATE,
      message: this.args.nameSpace,
      page: this.rsExportService.firstPageExport + "," + this.rsExportService.currentPDFCopy
    };

    this.rsService.pageNum = this.rsExportService.firstPageExport;
    this.rsService.doSend(JSON.stringify(message));
    this.rsExportService.currentPDFCopy++;
  }

  // -----------------------------------------------
  snapshot() {
    //il flag user-allUser è passato insieme al numeroPagina
    let message = {
      commandType: CommandType.SNAPSHOT,
      message: this.args.nameSpace,
      page: 1 + "," + this.rsExportService.nameSnap + "," + this.rsExportService.user
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  createTableSnapshots(k: Snapshot[]) {
    this.rsExportService.snapshots = k;
  }

  // -----------------------------------------------
  runSnapshot() {
    this.rsExportService.snapshot = false;
    let message = {
      commandType: CommandType.RUNSNAPSHOT,
      message: this.args.nameSpace,
      page: 1 + "," + this.rsExportService.dateSnap + "_" + this.rsExportService.nameSnap + "," + this.rsExportService.user
    };

    this.rsService.doSend(JSON.stringify(message));
  }
  // -----------------------------------------------
  closeReport() {
    this.rsService.close();
  }


  //--------------------------------------------------
  startSaveSVG() {
    this.rsExportService.svgState = SvgType.SVG;
    this.currentPage();
  }

  //--------------------------------------------------
  startSavePNG() {
    this.rsExportService.pngState = PngType.PNG;
    this.currentPage();
  }

  //--------------------------------------------------
  setExportFile(type: string) {
    if (type == this.rsExportService.pdf)
      this.rsExportService.exportpdf = true;
    if (type == this.rsExportService.excel)
      this.rsExportService.exportexcel = true;
    if (type == this.rsExportService.docx)
      this.rsExportService.exportdocx = true;
    this.rsExportService.exportfile = true
  }

  //--------------------------------------------------
  async startSavePDF() {
    this.rsExportService.pdfState = PdfType.PDF;
    this.pageNumber();
  }

  //--------------------------------------------------
  startSaveExcel() {
    let message = {
      commandType: CommandType.EXPORTEXCEL,
      message: this.args.nameSpace,
      page: this.rsExportService.firstPageExport + "," + this.rsExportService.lastPageExport
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  //--------------------------------------------------
  startSaveDocx() {
    let message = {
      commandType: CommandType.EXPORTDOCX,
      message: this.args.nameSpace,
      page: this.curPageNum
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  //-------------------------------------------------- 
  startSnapshot() {
    this.rsExportService.snapshot = true;
     this.httpServiceRs.getSnapshotData(this.args.nameSpace).subscribe( resp => this.createTableSnapshots(resp)   )
     //this.rsExportService.snapshots =this.httpServiceRs.snapArray;
  }

  //--------------------------------------------------
  getExcelData(filename: string) {
    var iframeHTML = document.getElementById('iframe') as HTMLFrameElement;
    var s = this.infoService.getReportServiceUrl() + 'excel/' + filename;
    iframeHTML.src = s;
  }

  //--------------------------------------------------
  getDocxData(filename: string) {
    var iframeHTML = document.getElementById('iframe') as HTMLFrameElement;
    var s = this.infoService.getReportServiceUrl() + 'docx/' + filename;
    iframeHTML.src = s;
  }

  getLayoutId() {

  }

}


@Component({
  template: ''
})
export class ReportingStudioFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver, public activatedRoute: ActivatedRoute) {
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

