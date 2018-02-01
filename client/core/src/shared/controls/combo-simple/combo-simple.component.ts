import { Component, OnInit, Input, OnChanges, Output, EventEmitter, ViewEncapsulation, ViewChild, HostListener, ChangeDetectionStrategy } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-combo-simple',
  templateUrl: './combo-simple.component.html',
  styleUrls: ['./combo-simple.component.scss'],
  changeDetection: ChangeDetectionStrategy.Default,
  encapsulation: ViewEncapsulation.None
})
export class ComboSimpleComponent extends ControlComponent {

  @Input() public items: Array<any> = [];
  @Input() public defaultItem: any;
  @Output('changed') changed: EventEmitter<any> = new EventEmitter();

  @Input() propagateSelectionChange = false;

  selectedItem: any;
  private oldValue: any;

  public selectionChange(value: any): void {
    if (this.propagateSelectionChange) {
      this.valueChange(value);
    }
  }

  @ViewChild("ddl") public dropdownlist: any;

  @HostListener('keydown', ['$event'])
  public keydown(event: any): void {
    if (event.target.id === this.dropdownlist.id) {
      switch (event.keyCode) {
        case 9: // tab
        case 13: // enter
          this.oldValue = this.selectedItem;
          break;
        case 27: // esc
          this.selectedItem = this.oldValue;
          break;
      }
    }
  }

  focus() {
    this.oldValue = this.selectedItem;
  }

  valueChange(value) {
    this.selectedItem = value;
    this.changed.emit(this);
  }
}

