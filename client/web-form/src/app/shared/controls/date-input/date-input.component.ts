import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from './../control.component';
import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'tb-date-input',
  templateUrl: './date-input.component.html',
  styles: [`./date-input.component.scss`]
})

export class DateInputComponent extends ControlComponent implements OnInit {

  @Input() objDate: Date;
  @Output() clicked = new EventEmitter<string>();

  public mask = 'dA / mA / yyyy';

  private switchP = false;
  value = '__ / __ / ____';
  public rules: { [key: string]: RegExp } = {
    'A': /[0-9]/,
    'd': /[0123]/,
    'm': /[012]/,
    'y': /[0-9]/
  };

  constructor(private eventData: EventDataService) {
    super();
  }

  ngOnInit() {
    this.eventData.command.subscribe(data => this.MySave(data));
  }

  public handleChange(value: Date): void {
    this.UpdateModel(value);
    this.onClickM();
  }

  onClickM() {
    this.switchP = !this.switchP;
  }

  onBlur() {
    this.onClickM();
  }

  UpdateModel(newDate: Date) {
    this.objDate = newDate;
    this.value = this.objDate.toLocaleDateString('en-GB');
  }

  MySave(data: string) {
    if (data !== 'ID_EXTDOC_SAVE') { return; }
    let y = new Date(this.objDate.getFullYear(), this.objDate.getMonth(), this.objDate.getDate(),
      12, this.objDate.getMinutes(), this.objDate.getSeconds());
    this.model.value = y.toJSON().substring(0, 19);
  }

  ngAfterViewInit() {
    this.UpdateModel(new Date(this.model.value));
  }

  ngOnChanges() {
    this.UpdateModel(new Date(this.model.value));
  }

}
