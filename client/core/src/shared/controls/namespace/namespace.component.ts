import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
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

@Component({
  selector: 'tb-namespace',
  templateUrl: './namespace.component.html',
  styleUrls: ['./namespace.component.scss']
})
export class NamespaceComponent extends ControlComponent implements OnChanges, OnInit {


  @Input('readonly') readonly: boolean = false;
  @Input('chars') chars: number = 0;
  @Input('textLimit') textlimit: number = 0;
  @Input('maxLength') maxLength: number = 524288;
  errorMessage: any;

  @ViewChild(ExplorerDialogComponent) explorer: ExplorerDialogComponent;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    private store: Store
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    this.store.select(_ => this.model && this.model.length).
      subscribe(l => this.onlenghtChanged(l));
  }

  onlenghtChanged(len: any) {
    if (len !== undefined)
      this.setlength(len);
  }

  onBlur($event) {
    if ($event == undefined)
      return;

    if (!this.validate()) {
      this.errorMessage = this._TB('INVALID: Incorrect namespace.');
      return;
    }

    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

  validate(): boolean {
    // Se non ci sono punti, non sono un namespace
    let elem = this.value.split(".");
    if (elem.length == 0)
      return false;

    // Se il primo token non è nei predefiniti non sono un namespace valido
    if (ObjType[elem[0]] === undefined)
      return false;

    // Guardo se trovo il file selezionato tramite le API

    return true;
  }

  // Metodo con OnChanges
  ngOnChanges(changes: SimpleChanges) {
    if (!changes.model || !this.model || !this.model.length)
      return;

    this.setlength(this.model.length)
  }

  setlength(len: number) {
    this.maxLength = this.model ? this.model.length : 0;
    if (this.textlimit > 0 && (this.maxLength == 0 || this.textlimit < this.maxLength)) {
      this.maxLength = this.textlimit;
    }
  }
}
