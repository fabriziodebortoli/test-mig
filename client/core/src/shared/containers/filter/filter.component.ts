import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { BOService } from './../../../core/services/bo.service';
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { TbComponent } from '../../components/tb.component';

@Component({
  selector: 'tb-filter',
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.scss']
})
export class FilterComponent extends TbComponent implements OnInit, OnDestroy {

  @Input() title: string;

  @Input() isPinned: boolean = true;
  @Input() isPinnable: boolean = false;

  @Output() toggle = new EventEmitter<boolean>();
  constructor(
    private boService: BOService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
  }
  ngOnInit() {
    this.boService.activateContainer(this.cmpId, true, false);
  }
  ngOnDestroy() {

    this.boService.activateContainer(this.cmpId, false, false);
  }
  togglePin(emit: boolean = true): void {
    if (!this.isPinnable) return;
    this.isPinned = !this.isPinned;
    if (emit) this.toggle.emit(this.isPinned);
  }
  
}
