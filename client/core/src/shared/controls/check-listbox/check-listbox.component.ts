import { Component, OnInit, Input, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams } from '@angular/http';

import { HttpService } from './../../../core/services/http.service';
import { ControlComponent } from './../control.component';
import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { RowArgs, SelectableSettings } from '@progress/kendo-angular-grid';

@Component({
  selector: 'tb-check-listbox',
  templateUrl: './check-listbox.component.html',
  styleUrls: ['./check-listbox.component.scss']
})
export class CheckListBoxComponent extends ControlComponent implements OnInit {

  @Input() hotLink: any;

  public gridData: any[];
  public mySelection: string[] = [];
  public mySelectionKey: string;

  constructor(public httpService: HttpService,
    private documentService: DocumentService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    public cd: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, cd);
  }

  ngOnInit() {
    this.readData();
  }

  public onSelectedKeysChange(e) {
    const len = this.mySelection.length;
    let result = this.mySelection.join(",");
    console.log(result);
    this.value = result;
    return result;
  }

  async readData() {
    let p: URLSearchParams = new URLSearchParams();
    p.set('documentID', (this.tbComponentService as DocumentService).mainCmpId);
    p.set('hklName', this.hotLink.name);
    // p.set('page', JSON.stringify(1));
    // p.set('per_page', JSON.stringify(10));
    let data = await this.httpService.getHotlinkData(this.hotLink.namespace, 'code', p).toPromise();
    this.mySelectionKey = data.key;
    this.gridData = data.rows;
  }
}
