import { baserect } from './../../../models/baserect.model';
import { rect } from './../../../models/rect.model';
import { barcode } from './../../../models/barcode.model';

import { Component, OnChanges, SimpleChanges, Input, ViewEncapsulation } from '@angular/core';
import bwipjs from 'bwip-angular2';

@Component({
  selector: 'rs-barcode',
  templateUrl: './barcode.component.html',
  styles: [],
  encapsulation: ViewEncapsulation.None
})
export class BarcodeComponent implements OnChanges {

  @Input() barcode: barcode;
  @Input() rect: baserect;
  @Input() value: string;

  constructor() { }

  ngOnChanges(changes: SimpleChanges) {
    if (!this.value)
      return;
    bwipjs('barcodeCanvas', {
      bcid: this.barcode.type,       // Barcode type
      text: this.value,   	             // Text to encode
      scale: 3,                 // 3x scaling factor
      height: this.rect.rect.bottom - this.rect.rect.top,  // Bar height, in millimeters
      width: this.rect.rect.left - this.rect.rect.right,
      includetext: this.barcode.includetext,        // Show human-readable text
      rotate: this.barcode.rotate,
      textxalign: 'center',      // Always good to set this
    }, function (err, cvs) {
      if (err) {
        document.getElementById('err').innerText = 'Error occured. See browser log for more information';
        console.log(err);
      } else {
      }
    });
  }
}