import { LayoutService } from '@taskbuilder/core';
import { ReportingStudioService } from './../../reporting-studio.service';
import { RsExportService } from './../../rs-export.service';

import { barcode } from './../../models/barcode.model';
import { chart, series } from './../../models/chart.model';
import { PdfType, SvgType, PngType } from './../../models/export-type.model';
import { column } from './../../models/column.model';
import { link } from './../../models/link.model';
import { graphrect } from './../../models/graphrect.model';
import { fieldrect } from './../../models/fieldrect.model';
import { textrect } from './../../models/textrect.model';
import { table } from './../../models/table.model';
import { sqrrect } from '../../models/sqrrect.model';
import { baseobj } from '../../models/baseobj.model';
import { repeater } from '../../models/repeater.model';
import { title } from './../../models/title.model';
import { gauge } from './../../models/gauge.model';
import { TemplateItem } from '../../models/template-item.model';

import { Component, OnInit, Input, OnChanges, SimpleChange, OnDestroy, Type, ElementRef, Renderer2 } from '@angular/core';
import { Subscription } from "rxjs/Subscription";
import { ReportObjectType } from '../../models/report-object-type.model';

@Component({
  selector: 'rs-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})

export class ReportLayoutComponent implements OnChanges, OnInit, OnDestroy {

  @Input() reportTemplate;
  @Input() reportData;

  public layoutStyle: any = {};
  public layoutBackStyle: any = {};
  public layoutObjectDivStyle: any = {};
  public objects: baseobj[] = [];
  public templates: TemplateItem[] = [];
  currentPage: number;

  public viewHeightSubscription: Subscription;
  public viewHeight: number;
  public showAsk: boolean;
  public ROT = ReportObjectType;

  constructor(public layoutService: LayoutService,
     public rsService: ReportingStudioService,
     public rsExportService: RsExportService, 
     public elRef: ElementRef, 
     public renderer: Renderer2) { }

  // -----------------------------------------------
  ngOnInit() {
    this.viewHeightSubscription = this.layoutService.getViewHeight().subscribe((viewHeight) => {
      this.viewHeight = viewHeight;
      if(this.elRef.nativeElement.firstElementChild !== null)
        this.renderer.setStyle(this.elRef.nativeElement.firstElementChild, 'height', this.viewHeight-76 + 'px',);
    });
    
    this.rsExportService.imageLoaded.subscribe(()=> this.createPDF());
  }

  // -----------------------------------------------
  ngOnDestroy() {
    this.viewHeightSubscription.unsubscribe();
  }

  // -----------------------------------------------
  ngOnChanges(changes: { [propKey: string]: SimpleChange }) {
    if (changes.reportTemplate !== undefined) {
      if (changes.reportTemplate.currentValue === 'empty') {
        this.reportTemplate = undefined;
        this.objects = [];
        this.templates = [];
      }
      else {
        this.RenderLayout();
      }
    }
    if (changes.reportData !== undefined) {
      if (changes.reportData.currentValue === 'empty') {
        this.reportData = undefined;
      }
      else {
        if (this.rsExportService.pdfState === PdfType.PDF) {
          if(this.rsExportService.imgCounter === 0)
            this.createPDF();
        }
        else this.UpdateData();
        if (this.rsExportService.svgState == SvgType.SVG) {
          this.rsExportService.exportSVG();
        }
        if (this.rsExportService.pngState == PngType.PNG) {
          this.rsExportService.exportPNG();
        }
      }
    }
  }

  // -----------------------------------------------
  async createPDF() {
    await this.UpdateData();
    if (this.rsService.pageNum == this.rsExportService.firstPageExport) {
      if (this.rsExportService.lastPageExport == this.rsExportService.firstPageExport) {
        this.rsExportService.renderPDF();
        return;
      }
    }

    if (this.rsService.pageNum != this.rsExportService.lastPageExport) {
      this.rsExportService.appendPDF().then(() => {
        this.rsExportService.eventNextPage.emit();
      });
    } else {
      this.rsExportService.renderPDF();
    }
  }

  // -----------------------------------------------
  CreateFakeRowsEmptyTable(t: table) {
    let columns = t.columns;
    let dummyRowsJson: any[] = [];

    for (let j = 0; j < t.row_number; j++) {
      let dummyRow = [];
      for (let i = 0; i < columns.length; i++) {
          const col: column = columns[i];
          if (col.hidden) {
            continue;
          }
          let idCell: string = col.id;
          let dummyCell = {};
          dummyCell[idCell] = { value : undefined };
          dummyRow.push(dummyCell);
        }
        dummyRowsJson.push(dummyRow);
    }
    t.value = dummyRowsJson;
  }

  // -----------------------------------------------
  RenderLayout() {
    if (this.reportTemplate === undefined) {
      return;
    }

    let template = this.FindTemplate(this.reportTemplate.page.layout_name);
    if (template !== undefined) {
      this.CleanImageSrc(template.templateObjects);
      this.objects = template.templateObjects;
      this.setDocumentStyle(template.template.page);
      return;
    }

    let objects = [];
    this.setDocumentStyle(this.reportTemplate.page);

    for (let index = 0; index < this.reportTemplate.page.layout.objects.length; index++) {
      let element = this.reportTemplate.page.layout.objects[index];
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
        this.CreateFakeRowsEmptyTable(obj);
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
      else if (element.chart !== undefined) {
        obj = new chart(element.chart);
      }
      else if (element.gauge !== undefined) {
        obj = new gauge(element.gauge);
      }
      else //skip unknown objects
        continue;

      objects.push(obj);
    }

    //if (!this.ExistsTemplate(this.reportTemplate.page.layout.name))
      this.templates.push(new TemplateItem(this.reportTemplate.page.layout_name, this.reportTemplate, objects));

    this.objects = objects;
    return;
  }

  // -----------------------------------------------
  UpdateData() {
    if (this.reportData === undefined) {
      return;
    }

    if (this.rsService.pageNum !== this.reportData.page.page_number) {
      return;
    }
    let id: string;
    let value: any;
    for (let index = 0; index < this.reportData.page.layout.objects.length; index++) {
      let element = this.reportData.page.layout.objects[index];
      try {
        if (element.fieldrect !== undefined) {
          // FIELDRECT
          id = element.fieldrect.baserect.baseobj.id;
          value = element.fieldrect.value;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
          }
          const h = element.fieldrect.baserect.baseobj.hidden;
          if (h !== undefined && h !== obj.hidden) {
            obj.hidden = h;
          }
          if (element.fieldrect.label !== undefined) {
            obj.label.caption = element.fieldrect.label.caption;
            if (element.fieldrect.label.textcolor !== undefined) {
              obj.label.textcolor = element.fieldrect.label.textcolor;
            }
          }
          if (element.fieldrect.link !== undefined) {
            obj.link = new link(element.fieldrect.link);
          }
          if (element.fieldrect.textcolor !== undefined) {
            obj.textcolor = element.fieldrect.textcolor;
          }
          if (element.fieldrect.bkgcolor !== undefined) {
            obj.bkgcolor = element.fieldrect.bkgcolor;
          }
          obj.value = value;
        }
        else if (element.textrect !== undefined) {
          //TEXTRECT
          id = element.textrect.baserect.baseobj.id;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
          }
          const h = element.textrect.baserect.baseobj.hidden;
          if (h !== undefined && h !== obj.hidden) {
            obj.hidden = h;
          }
          if (element.textrect.value !== undefined) {
            obj.value = element.textrect.value;
          }
          if (element.textrect.textcolor !== undefined) {
            obj.textcolor = element.textrect.textcolor;
          }
          if (element.textrect.bkgcolor !== undefined) {
            obj.bkgcolor = element.textrect.bkgcolor;
          }
        }
        else if (element.table !== undefined) {
          //TABLE
          id = element.table.baseobj.id;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
          }
          const h = element.table.baseobj.hidden;
          if (h !== undefined && h !== obj.hidden) {
           obj.hidden = h;
          }
          value = element.table.rows;
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
            if (source.width !== undefined) {
              target.width = source.width;
            }
            if (source.title !== undefined) {
              if (source.title.textcolor !== undefined) {
                target.title.textcolor = source.title.textcolor;
              }
              if (source.title.bkgcolor !== undefined) {
                target.title.bkgcolor = source.title.bkgcolor;
              }
            }
          }
          obj.value = value;
        }
        else if (element.chart !== undefined) {
          //CHART
          id = element.chart.baserect.baseobj.id;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
          }
          const h = element.chart.baserect.baseobj.hidden;
         if (h !== undefined && h !== obj.hidden){
          obj.hidden = h;
         }
          if (obj.series)
            obj.series = [];
          element.chart.series.forEach(element => {
            obj.series.push(new series(element));
          });

          if (element.chart.category_axis) {
            obj.category_title = element.chart.category_axis.title;
            obj.categories = element.chart.category_axis.categories;
          }
        }
        else if (element.gauge !== undefined) {
          //GAUGE
          id = element.gauge.baserect.baseobj.id;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
          }
          const h = element.gauge.baserect.baseobj.hidden;
         if (h !== undefined && h !== obj.hidden){
          obj.hidden = h;
         }
          if (element.gauge.pointers && obj.pointers)
          {
            for (let i=0;i<element.gauge.pointers.length;i++){
              let pointer=element.gauge.pointers[i];
              obj.pointers[i].value=pointer.value;
            }
          }
        }
        else if (element.sqrrect !== undefined) {
          //SQRRECT
          id = element.sqrrect.baserect.baseobj.id;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
          }
          const h = element.sqrrect.baserect.baseobj.hidden;
          if (h !== undefined && h !== obj.hidden){
            obj.hidden = h;
        }
      }
      else if (element.graphrect !== undefined) {
        //GRAPHRECT - image
        id = element.graphrect.baserect.baseobj.id;
        let obj = this.FindObj(id);
        if (obj === undefined) {
          continue;
        }
        const h = element.graphrect.baserect.baseobj.hidden;
        if (h !== undefined && h !== obj.hidden){
          obj.hidden = h;
      }
    }
  } catch (a) {
        console.log(a);
        let k = a;
      }
    }
  }

  // -----------------------------------------------
  setDocumentStyle(layout: any) {

    this.layoutStyle = {
      'width': layout.pageinfo ? (layout.pageinfo.width) + 'mm': 'inherit',
      'height': layout.pageinfo ? (layout.pageinfo.length) + 'mm': 'inherit',
      'background-color': 'white',
      'border': '1px solid #ccc',
      'position': 'relative',
      'margin': '5px auto',
      'padding-top': layout.pageinfo ? (layout.pageinfo.margin.top) + 'px': '0px',
      'padding-left': layout.pageinfo ? (layout.pageinfo.margin.left) + 'px': '0px' ,
    }

    if (this.rsExportService.pdfState === PdfType.NOPDF 
      || this.rsExportService.svgState === SvgType.NOSVG 
      || this.rsExportService.pngState === PngType.NOPNG) {
      this.layoutBackStyle = {
        'width': '100%',
        'height': this.viewHeight - 76+ 'px',
        'position': 'relative',
        'overflow': 'scroll',
   
      }
    }

    if (this.rsExportService.pdfState === PdfType.PDF 
      || this.rsExportService.svgState === SvgType.SVG 
      || this.rsExportService.pngState === PngType.PNG) {
      this.layoutBackStyle = {
        'overflow': 'hidden'
      }
    }
    this.layoutObjectDivStyle = {
      'position':'relative',
    }
  }

  // -----------------------------------------------
  public FindObj(id: string): any {
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
  public FindTemplate(name: string) : TemplateItem {
    for (let index = 0; index < this.templates.length; index++) {
      if (this.templates[index].templateName === name) {
        return this.templates[index];
      }
    }
    return undefined;
  }

  public ExistsTemplate(name: string) : boolean {
    for (let index = 0; index < this.templates.length; index++) {
      if (this.templates[index].templateName === name) {
        return true;
      }
    }
    return false;
  }

  CleanImageSrc(objects: any[]){
    for (let index = 0; index < this.objects.length; index++) {
      let element = objects[index];
      if(element instanceof graphrect)
      {
        element.src = "";
      }
    }

  }
}

