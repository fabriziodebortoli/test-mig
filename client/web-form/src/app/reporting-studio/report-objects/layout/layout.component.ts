import { LayoutService } from '@taskbuilder/core';
import { ReportingStudioService } from './../../reporting-studio.service';
import { TemplateItem, column, link, graphrect, fieldrect, textrect, table, sqrrect, baseobj, PdfType } from '@taskbuilder/reporting-studio';
import { Component, OnInit, Input, OnChanges, SimpleChange, OnDestroy } from '@angular/core';
import { Subscription } from "rxjs/Subscription";

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

  private viewHeightSubscription: Subscription;
  private viewHeight: number;

  constructor(private layoutService: LayoutService, private rsService: ReportingStudioService) { }

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
        this.objects = undefined;
        this.templates = undefined;
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
        this.UpdateData();
        if (this.rsService.pdfState == PdfType.SAVINGPDF) {
          this.createPDF();
        }
      }
    }
  }

  // -----------------------------------------------
  createPDF() {
    if (this.rsService.pageNum == 1) {
      if(this.rsService.totalPages == 1){
      this.rsService.renderPDF();
      return;
    }
      this.rsService.eventNextPage.emit();
      return;
    }

    this.rsService.appendPDF().then(() => {
      if (this.rsService.pageNum != this.rsService.totalPages) {
        this.rsService.eventNextPage.emit();
      }
      else {
        this.rsService.renderPDF();
      }
    });

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
      'position': 'relative',
    }
    if (this.rsService.pdfState == PdfType.NOPDF) {
      this.layoutBackStyle = {
        'width': '100%',
        'height': this.viewHeight - 65 + 'px',
        'position': 'relative',
        'overflow': 'scroll'
      }
    }

    if (this.rsService.pdfState == PdfType.SAVINGPDF) {
      this.layoutBackStyle = {
        'overflow': 'hidden'
      }
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

