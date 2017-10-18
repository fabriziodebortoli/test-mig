import { baserect } from './../../../models/baserect.model';
import { rect } from './../../../models/rect.model';
import { barcode } from './../../../models/barcode.model';

import { Component, OnChanges, SimpleChanges, Input, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import bwipjs from 'bwip-angular2';

@Component({
  selector: 'rs-barcode',
  templateUrl: './barcode.component.html',
  styles: [`
    #myimg{
      margin-top:auto;
      margin: auto;
      display: block;
    }
  `],

  encapsulation: ViewEncapsulation.None
})
export class BarcodeComponent implements OnChanges {

  @Input() barcode: barcode;
  @Input() rect: baserect;
  @Input() value: string;

  //private scaleY:number;

  constructor(private ref: ChangeDetectorRef) {
    setInterval(() => {
      // the following is required, otherwise the view will not be updated
      this.ref.markForCheck();
    }, 1000);
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!this.rect || !this.value) {
      return;
    }
    let scale = this.CreateBarcode();
    this.DrawBarcode(scale);
  }

  CreateBarcode(): number {

    let width = this.rect.rect.right - this.rect.rect.left;
    let height = this.rect.rect.bottom - this.rect.rect.top;
    let canvas = document.createElement('canvas');
    let sc: number;
    bwipjs(canvas, {
      bcid: this.barcode.type,       // Barcode type
      text: this.value,//this.value,   	             // Text to encode
      //scale: 2,                 // 3x scaling factor
      //height: ((this.rect.rect.bottom - this.rect.rect.top) * 25.4) / 160,  // Bar height, in millimeters
      // width: ((this.rect.rect.right - this.rect.rect.left) * 25.4) / 160,
      includetext: this.barcode.includetext,        // Show human-readable text
      rotate: this.barcode.rotate,
      textxalign: 'center',      // Always good to set this
    }, function (err, cvs) {
      if (err) {
        console.log(err);
        sc = 0;
      } else {
        sc = Math.floor(Math.min(width / canvas.width, height / canvas.height)) + 2;
      }
    });

    return sc;
  }

  DrawBarcode(scale: number) {

    let canvas = document.createElement('canvas');
    bwipjs(canvas, {
      bcid: this.barcode.type,       // Barcode type
      text: this.value,//this.value,   	             // Text to encode
      scale: scale,                 // 3x scaling factor
      //height: ((this.rect.rect.bottom - this.rect.rect.top) * 25.4) / 166,  // Bar height, in millimeters
     // width: ((this.rect.rect.right - this.rect.rect.left) * 25.4) / 166,
      includetext: this.barcode.includetext,        // Show human-readable text
      rotate: this.barcode.rotate,
      textxalign: 'center',      // Always good to set this
    }, function (err, cvs) {
      if (err) {
        document.getElementById('err').innerText = 'Error occured. See browser log for more information';
        console.log(err);
      } else {
        document.getElementById('myimg').setAttribute('src', canvas.toDataURL('image/png'));
      }
    })
  }

  private IfBarcodeAquared(type: string): boolean {
    switch (type) {
      case 'qrcode':
      case 'datamatrix':
      case 'microqrcode':
        return true;

      default:
        return false;
    }
  }
}
