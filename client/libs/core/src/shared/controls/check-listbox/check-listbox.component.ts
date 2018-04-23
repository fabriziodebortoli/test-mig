import { Component, OnInit, Input, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams } from '@angular/http';

import { HttpService } from './../../../core/services/http.service';
import { ControlComponent } from './../control.component';
import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { RowArgs, SelectableSettings } from '@progress/kendo-angular-grid';

import { Store } from './../../../core/services/store.service';

@Component({
  selector: 'tb-check-listbox',
  templateUrl: './check-listbox.component.html',
  styleUrls: ['./check-listbox.component.scss']
})
export class CheckListBoxComponent extends ControlComponent implements OnInit {

  public gridData: any[];
  public mySelection: string[] = [];
  public mySelectionKey: string;
  public visible: boolean = true;

  constructor(public httpService: HttpService,
    private documentService: DocumentService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    public changeDetectorRef: ChangeDetectorRef,
    private store: Store
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    this.readData();

    this.store.select(_ => this.model && this.model.value).
      subscribe(v => this.onvalueChanged(v));
  }

  onvalueChanged(value: any) {
    if (value !== undefined) {
      this.mySelection = this.value.map(x => x._value);
    }
  }

  public onSelectedKeysChange(e) {
    let result = this.mySelection.map(obj => ({ ['_value']: obj }));

    this.value = result;
    return result;
  }

  async readData() {
    let p: URLSearchParams = new URLSearchParams();
    p.set('documentID', (this.tbComponentService as DocumentService).mainCmpId);
    p.set('hklName', this.hotLink.name);
    let data = await this.httpService.getHotlinkData(this.hotLink.namespace, 'code', p).toPromise();
    this.mySelectionKey = data.key;
    this.gridData = data.rows;

    // Non faccio apparire il control se non ho record
    // Andra' fatta sparire tutta la filter tile
    this.visible = data.rows.length > 0;

    this.changeDetectorRef.detectChanges();
  }
}
