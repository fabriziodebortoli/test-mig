import { EventDataService } from './../../../core/eventdata.service';
import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { Align } from "@progress/kendo-angular-popup/dist/es/models/align.interface";

@Component({
  selector: 'tb-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.scss']
})

export class DateInputComponent extends ControlComponent implements OnInit {
  @Input() forCmpID: string;
  @Output() clicked = new EventEmitter<string>();

  private anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  private popupAlign: Align = { horizontal: 'left', vertical: 'center' };
  public mask = 'dA / mA / yyyy';
  private objDate: Date;
  private switchP = false;
  value = '__ / __ / ____';
  public rules: { [key: string]: RegExp } = {
    'A': /[0-9]/,
    'd': /[0123]/,
    'm': /[01]/,
    'y': /[0-9]/
  };

  constructor(private eventData: EventDataService) {
    super();
  }

  ngOnInit() {
    this.eventData.command.subscribe(data => this.onSave(data));
  }

  public handleChange(value: Date): void {
    this.onUpdateModel(value);
    this.onClickM();
  }

  onClickM() {
    this.switchP = !this.switchP;
  }

  onClose() {
    this.switchP = false;
  }

  onBlur() {
    this.onClickM();
  }

  onUpdateModel(newDate: Date) {
    this.objDate = newDate;
    this.value = this.objDate.toLocaleDateString('en-GB');
  }

  onSave(data: string) {
    if (data !== 'ID_EXTDOC_SAVE') { return; }
    let y = new Date(this.objDate.getFullYear(), this.objDate.getMonth(), this.objDate.getDate(),
      12, this.objDate.getMinutes(), this.objDate.getSeconds());
    this.model.value = y.toJSON().substring(0, 19);
  }

  ngAfterViewInit() {
    if (this.model !== undefined && this.model !== null)
      this.onUpdateModel(new Date(this.model.value));
  }

  ngOnChanges() {
    if (this.model !== undefined && this.model !== null)
      this.onUpdateModel(new Date(this.model.value));
  }

}
