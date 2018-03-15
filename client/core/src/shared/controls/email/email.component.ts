import { ControlContainerComponent } from './../control-container/control-container.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input, OnInit, OnChanges, AfterViewInit, ChangeDetectorRef, ViewChild } from '@angular/core';

import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from './../control.component';


@Component({
  selector: 'tb-email',
  templateUrl: './email.component.html',
  styleUrls: ['./email.component.scss']
})

export class EmailComponent extends ControlComponent implements OnInit, OnChanges, AfterViewInit {
  @Input('readonly') readonly: boolean = false;
  mask = '';
  public constraint: RegExp;
  //TODOLUCA, aggiungere derivazione da textedit, e spostare rows e chars come gestione nel componente text
  @Input('rows') rows: number = 0;
  @Input('chars') chars: number = 0;

  @ViewChild(ControlContainerComponent) cc: ControlContainerComponent;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
  }

  public onChange(val: any) {
    this.onUpdateNgModel(val);
  }

  onUpdateNgModel(newValue: string): void {
    if (!this.modelValid()) {
      this.model = { enable: 'true', value: '' };
    }
    this.model.value = newValue;

  }

  ngAfterViewInit(): void {
    if (this.modelValid()) {
      this.onUpdateNgModel(this.model.value);
    }
  }

  ngOnChanges(): void {
    if (this.modelValid()) {
      this.onUpdateNgModel(this.model.value);
    }
  }
  modelValid() {
    return this.model !== undefined && this.model !== null;
  }

  onMailClick() {
    if (this.model.enabled || this.model.value.trim().length == 0)
      return;
    location.href = "mailto:" + this.model.value;
    return 0;
  }

  onBlur(e): any {
    this.constraint = new RegExp('^[a-zA-Z0-9_\+-]+(\.[a-zA-Z0-9_\+-]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.([a-zA-Z]{2,})$', 'i');
    var arEmail = this.model.value.split(";");
    this.cc.errorMessage = '';
    if (arEmail.length > 0) {
      for (var i = 0; i < arEmail.length; i++) {
        if (!this.constraint.test(arEmail[i].trim())) {
          this.cc.errorMessage = 'Input not in correct form';
          break;
        }
      }
    }

    this.eventData.change.emit(this.cmpId);
  }
}
