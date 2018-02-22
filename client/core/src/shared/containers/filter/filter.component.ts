import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'tb-filter',
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.scss']
})
export class FilterComponent {

  @Input() title: string;

  @Input() isPinned: boolean = true;
  @Input() isPinnable: boolean = false;

  @Output() toggle = new EventEmitter<boolean>();

  togglePin(emit: boolean = true): void {
    if (!this.isPinnable) return;
    this.isPinned = !this.isPinned;
    if (emit) this.toggle.emit(this.isPinned);
  }
  
}
