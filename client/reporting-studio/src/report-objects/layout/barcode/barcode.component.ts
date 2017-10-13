
import { Component, OnChanges, SimpleChanges, Input, ViewEncapsulation } from '@angular/core';
import bwipjs from 'bwip-angular2';

@Component({
  selector: 'rs-barcode',
  templateUrl: './barcode.component.html',
  styles: [],
  encapsulation: ViewEncapsulation.None
})
export class BarcodeComponent implements OnChanges {

  @Input() barcode: any;

  constructor() {}

  ngOnChanges(changes: SimpleChanges) {
    bwipjs('barcodeCanvas', {
      bcid: 'datamatrix',       // Barcode type
      text: this.barcode,   	  // Text to encode
      scale: 3,                 // 3x scaling factor
      height: 10,               // Bar height, in millimeters
      width: 10,
      includetext: true,        // Show human-readable text
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