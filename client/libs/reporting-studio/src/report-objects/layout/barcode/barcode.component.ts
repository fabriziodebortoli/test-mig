import { ReportingStudioService } from './../../../reporting-studio.service';
import { baserect } from './../../../models/baserect.model';
import { rect } from './../../../models/rect.model';
import { barcode } from './../../../models/barcode.model';

import { Component, OnChanges, SimpleChanges, Input, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
//import bwipjs from 'bwip-angular2';

@Component({
  selector: 'rs-barcode',
  templateUrl: './barcode.component.html',
  styleUrls: ['./barcode.component.css']
})
export class BarcodeComponent implements OnChanges {

  @Input() barcode: barcode;
  @Input() rect: baserect;
  @Input() value: string;
  public id: string;
  private dpi: number;

  constructor(private ref: ChangeDetectorRef,
    public rsService: ReportingStudioService, ) {
    setInterval(() => {
      // the following is required, otherwise the view will not be updated
      this.ref.markForCheck();
    }, 1000);

    this.id = this.rsService.generateId();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!this.rect || !this.value) {
      return;
    }
    let devicePixelRatio = window.devicePixelRatio || 1;
    this.dpi = Math.min((document.getElementById('testdiv').offsetWidth * devicePixelRatio), (document.getElementById('testdiv').offsetHeight * devicePixelRatio));
    this.CreateBarcode();
  }

  CreateBarcode() {

    let Width: number;
    let Height: number;

    if (this.IfBarcodeSquared(this.barcode.type)) {
      Width = Height = Math.min((this.rect.rect.right - this.rect.rect.left), (this.rect.rect.bottom - this.rect.rect.top));
    }
    else {
      Width = this.rect.rect.right - this.rect.rect.left;
      Height = this.rect.rect.bottom - this.rect.rect.top;
    }

    let Id = this.id;
   /* bwipjs(this.id, {
      bcid: this.barcode.type,       // Barcode type
      text: this.value,	             // Text to encode
      height: (Height * 25.4) / (this.dpi || 96),
      width: (Width * 25.4) / (this.dpi || 96),
      scale: 1,
      includetext: this.barcode.type == 'ean13' ? this.barcode.includetext : false,//this.barcode.includetext,        // Show human-readable text
      rotate: this.barcode.rotate,
      textxalign: 'center',      // Always good to set this
    }, function (err, cvs) {
      if (err) {
        console.log(err);
      } else {
      }
    });*/
  }

  IncludeText() {
    switch (this.barcode.type) {
      case 'qrcode':
      case 'datamatrix':
      case 'microqrcode':
      case 'pdf417':
      case 'ean13':
        return false;

      default:
        return this.barcode.includetext;
    }
  }

  private IfBarcodeSquared(type: string): boolean {
    switch (type) {
      case 'qrcode':
      case 'datamatrix':
      case 'microqrcode':
        return true;

      default:
        return false;
    }
  }

  private GetInitialScale(type: string): number {
    switch (type) {
      case 'qrcode':
      case 'datamatrix':
      case 'microqrcode':
      case 'pdf417':
      case 'code39':
        return 1;

      default:
        return 0;
    }
  }
}
