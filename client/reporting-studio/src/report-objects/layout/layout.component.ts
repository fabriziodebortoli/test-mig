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

import { Component, OnInit, Input, OnChanges, SimpleChange, OnDestroy } from '@angular/core';
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
  public objects: baseobj[] = [];
  public templates: TemplateItem[] = [];
  currentPage: number;

  public viewHeightSubscription: Subscription;
  public viewHeight: number;
  public showAsk: boolean;
  public ROT = ReportObjectType;

  constructor(public layoutService: LayoutService, public rsService: ReportingStudioService, public rsExportService: RsExportService) { }

  // -----------------------------------------------
  ngOnInit() {
    this.viewHeightSubscription = this.layoutService.getViewHeight().subscribe((viewHeight) => this.viewHeight = viewHeight);
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
        if (this.rsExportService.pdfState == PdfType.PDF) {
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

    let template = this.FindTemplate(this.reportTemplate.page.layout.name);
    if (template !== undefined) {
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

    this.templates.push(new TemplateItem(this.reportTemplate.page.layout.name, this.reportTemplate, objects));
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
          // let caption = element.fieldrect.label.caption;
          id = element.fieldrect.baserect.baseobj.id;
          value = element.fieldrect.value;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
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
          //obj.label.caption = caption;
          obj.value = value;
        }
        else if (element.textrect !== undefined) {
          id = element.textrect.baserect.baseobj.id;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
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
          id = element.chart.baserect.baseobj.id;
          let obj = this.FindObj(id);
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
          id = element.gauge.baserect.baseobj.id;
          let obj = this.FindObj(id);
          if (obj === undefined) {
            continue;
          }
          if (element.gauge.pointers && obj.pointers)
          {
            for (let i=0;i<element.gauge.pointers.length;i++){
              let pointer=element.gauge.pointers[i];
              obj.pointers[i].value=pointer.value;
            }
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
      'width': (layout.pageinfo.width) + 'mm',
      'height': (layout.pageinfo.length) + 'mm',
      'background-color': 'white',
      'border': '1px solid #ccc',
      'margin': '5px auto',
      'position': 'relative',
    }
    if (this.rsExportService.pdfState == PdfType.NOPDF || this.rsExportService.svgState == SvgType.NOSVG || this.rsExportService.pngState == PngType.NOPNG) {
      this.layoutBackStyle = {
        'width': '100%',
        'height': this.viewHeight - 65 + 'px',
        'position': 'relative',
        'overflow': 'scroll'
      }
    }

    if (this.rsExportService.pdfState == PdfType.PDF || this.rsExportService.svgState == SvgType.SVG || this.rsExportService.pngState == PngType.PNG) {
      this.layoutBackStyle = {
        'overflow': 'hidden'
      }
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
  public FindTemplate(name: string): TemplateItem {
    for (let index = 0; index < this.templates.length; index++) {
      if (this.templates[index].templateName === name) {
        return this.templates[index];
      }
    }
    return undefined;
  }
}

