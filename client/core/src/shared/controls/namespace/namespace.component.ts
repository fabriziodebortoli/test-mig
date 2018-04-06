import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { ExplorerService } from './../../../core/services/explorer.service';
import {
  Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef,
  OnChanges, AfterContentInit, OnInit, Output, HostListener, EventEmitter, ChangeDetectorRef, SimpleChanges
} from '@angular/core';
import { Store } from './../../../core/services/store.service';

import { EventDataService } from './../../../core/services/eventdata.service';
import { ObjType } from '../../../core/services/explorer.service';

import { ControlComponent } from '../control.component';
import { Subscription } from '../../../rxjs.imports';
import { ExplorerDialogComponent } from '../../components/explorer/explorer-dialog.component';
import { FormMode, createSelector, createSelectorByPaths } from './../../../shared/shared.module';
import { Selector } from './../../../shared/models/store.models';

@Component({
  selector: 'tb-namespace',
  templateUrl: './namespace.component.html',
  styleUrls: ['./namespace.component.scss']
})
export class NamespaceComponent extends ControlComponent implements OnChanges, OnInit {


  @Input('readonly') readonly: boolean = false;
  @Input('chars') chars: number = 0;
  @Input('rows') rows: number = 0;
  @Input('objType') objType: string = "Report";
  @Input('textLimit') textlimit: number = 0;
  @Input('maxLength') maxLength: number = 524288;
  @Input() slice: any;
  @Input() selector: Selector<any, any>;

  errorMessage: any;
  objectType: ObjType = ObjType.Report;
  user: string;
  company: string;
  culture:string;

  @ViewChild(ExplorerDialogComponent) explorer: ExplorerDialogComponent;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    private explorerService: ExplorerService,
    private store: Store
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    this.store.select(_ => this.model && this.model.length).
      subscribe(l => this.onlenghtChanged(l));

    // if (this.selector) {
    //   this.store.select('objType'
    //     this.selector.nest('objType.value')
    //   ).subscribe(o => this.onObjectTypeChanced(o));
    // } else {
    //   console.log('Missing Selector in ' + this.cmpId);
    // }

    this.user = localStorage.getItem('_user');
    this.company = localStorage.getItem('_company');
    this.culture = localStorage.getItem('ui_culture');

    if (this.objType != undefined) {
      this.objectType = ObjType[this.objType];
      if (this.objectType === undefined)
        console.log('Wrong objectType in ' + this.cmpId);
    }    
  }

  // onObjectTypeChanced(objtype: string) {
  //   this.objectType = ObjType[objtype];
  //   if (this.objectType === undefined)
  //     console.log('Wrong objectType in ' + this.cmpId);
  // }

  search($event) {
    this.explorer.open({ objType: this.objectType })
      .subscribe(this.onsearch);
  }

  onsearch = async obj => {
    if (obj.items.length > 0) {
      this.model.value = ObjType[this.objectType] + '.' + obj.items[0].namespace;
      // if (this.selector) {
      //   const slice = await this.store.select(this.selector).take(1).toPromise();
      //   if (slice.description)
      //     slice.description.value = obj.items[0].name;
      // }
    }
  }

  onlenghtChanged(len: any) {
    if (len !== undefined)
      this.setlength(len);
  }

  onBlur($event) {
    if (!this.validate()) {
      this.errorMessage = this._TB('Incorrect namespace.');
      return;
    }

    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

  async validate(): Promise<boolean> {
    this.errorMessage = '';
    // Se non ci sono punti, non sono un namespace
    const elem = this.value.split('.');
    if (elem.length === 0)
      return false;

    // Se il primo token non è nei predefiniti non sono un namespace valido
    if (ObjType[elem[0]] === undefined)
      return false;

    // Guardo se trovo il file selezionato tramite le API
    let val = elem.slice(1).join('.');
    let res = await this.explorerService.GetByUser(this.objectType, val, this.user);

    return true;
  }

  // Metodo con OnChanges
  ngOnChanges(changes: SimpleChanges) {
    if (!changes.model || !this.model)
      return;



    this.setlength(this.model.length)
  }

  setlength(len: number) {
    if (this.model && this.model.length > 0)
      this.maxLength = this.model.length;

    if (this.textlimit > 0 && (this.maxLength == 0 || this.textlimit < this.maxLength)) {
      this.maxLength = this.textlimit;
    }
  }
}
