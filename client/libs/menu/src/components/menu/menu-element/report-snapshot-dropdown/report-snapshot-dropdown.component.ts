import { Component, Input, ViewEncapsulation, OnDestroy, TemplateRef, ElementRef, HostListener, ViewChild, OnInit, ChangeDetectorRef, AfterViewInit } from '@angular/core';

import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { PopupRef, PopupService } from '@progress/kendo-angular-popup';

import { TbComponent, TbComponentService, RsSnapshotService } from '@taskbuilder/core';

@Component({
  selector: 'tb-report-snapshot-dropdown',
  templateUrl: './report-snapshot-dropdown.component.html',
  styleUrls: ['./report-snapshot-dropdown.component.scss']
})

export class ReportSnapshotDropdownComponent extends TbComponent implements OnDestroy, AfterViewInit {
  private popupRef: PopupRef;

  @ViewChild('anchor', { read: ElementRef }) public anchor: ElementRef;
  @ViewChild('template', { read: TemplateRef }) public template: TemplateRef<any>;
  @Input() object: any;
  @Input() snapshots;

  elRef: HTMLElement;
  offsetLeft: any;
  offsetTop: any;

  constructor(elRef: ElementRef,
    public popupService: PopupService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    public rsSnapshotService: RsSnapshotService
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
    this.elRef = elRef.nativeElement;
  }

  @HostListener('document:click', ['$event'])
  public documentClick(event: any): void {
    if (!this.contains(event.target)) {
      this.close();
    }
  }

  //--------------------------------------------------------------------------------
  ngOnDestroy() {
    if (this.popupRef)
      this.popupRef.close();
    super.ngOnDestroy();
  }
  ngAfterViewInit() {
  }
  close() {
    if (this.popupRef) {
      this.popupRef.close();
      this.popupRef = null;
    }
  }

  onRunAndClose() {
    this.close();
  }

  onShowList(isNotEmpty: boolean) {
    // this.listSnap = isNotEmpty;
    // this.expandIconVisible = this.listSnap;
  }

  //--------------------------------------------------------------------------------
  private contains(target: any): boolean {
    return this.elRef.contains(target); 
  }

  //--------------------------------------------------------------------------------
  public togglePopup() {
    this.offsetLeft = this.elRef.getBoundingClientRect().left + 3;
    this.offsetTop = this.elRef.getBoundingClientRect().top - 4;
    if (this.popupRef) {
      this.popupRef.close();
      this.popupRef = null;
    } else {
      this.popupRef = this.popupService.open({
        content: this.template,
        offset: { top: this.offsetTop, left: this.offsetLeft },
        anchorAlign: { horizontal: 'right', vertical: 'bottom' },
        popupAlign: { horizontal: 'right', vertical: 'top' },
        popupClass: 'arrow-right'
      });
    }
  }
}
