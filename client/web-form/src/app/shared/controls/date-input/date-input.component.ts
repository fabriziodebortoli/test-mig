import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-date-input',
  templateUrl: './date-input.component.html',
  styles: [``]
})

export class DateInputComponent extends ControlComponent implements OnInit {

  @Input() forCmpID: string;
  public mask = '00 / 00 / 0000';
  private objDate: Date;
  private switchP = false;
  value = '__ / __ / ____';

 @Output() clicked = new EventEmitter<string>();

 public handleChange(value: Date): void {
       this.objDate = value;
       this.value = value.toLocaleDateString('en-GB');
       this.onClickM();
    }

  onClickM() {
    this.switchP = !this.switchP;
  }

  ngOnInit() {
  }

}
