import { baserect } from './../../../models/baserect.model';
import { rect } from './../../../models/rect.model';
import { barcode } from './../../../models/barcode.model';

import { Component, OnChanges, SimpleChanges, Input, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import bwipjs from 'bwip-angular2';

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


  constructor(private ref: ChangeDetectorRef) {
    setInterval(() => {
      // the following is required, otherwise the view will not be updated
      this.ref.markForCheck();
    }, 1000);

    this.id = this.GenerateId();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!this.rect || !this.value) {
      return;
    }
    let scale = this.CreateBarcode();
    this.DrawBarcode(scale[0], scale[1]);
  }

  CreateBarcode(): number[] {

    let width: number;
    let height: number;
    if (this.IfBarcodeSquared(this.barcode.type)) {
      width = height = Math.min((this.rect.rect.right - this.rect.rect.left), (this.rect.rect.bottom - this.rect.rect.top));
    }
    else {
      width = this.rect.rect.right - this.rect.rect.left;
      height = this.rect.rect.bottom - this.rect.rect.top;
    }

    let canvas = document.createElement('canvas');
    let sc: number[] = [1, 1];
    bwipjs(canvas, {
      bcid: this.barcode.type,       // Barcode type
      text: this.value,	             // Text to encode
      scale: 1,
      includetext: this.barcode.includetext,        // Show human-readable text
      rotate: this.barcode.rotate,
      textxalign: 'center',      // Always good to set this
    }, function (err, cvs) {
      if (err) {
        console.log(err);
        //sc[0] = sc[1] = 0;
      } else {
        if (canvas.width < width) {
          sc[0] = Math.floor(width / canvas.width);
        }
        if (canvas.height < height) {
          sc[1] = Math.floor(height / canvas.height);
        }
      }
    });

    return sc;
  }

  DrawBarcode(scX: number, scY: number) {

    bwipjs(this.id, {
      bcid: this.barcode.type,       // Barcode type
      text: this.value,//this.value,   	             // Text to encode
      scaleX: scX,                 // scaling
      scaleY: scY,                 // scaling
      //height: ((this.rect.rect.bottom - this.rect.rect.top) * 25.4),  // Bar height, in millimeters
      //width: ((this.rect.rect.bottom - this.rect.rect.top) * 25.4),
      //paddingheight: 2,
      includetext: this.barcode.includetext,        // Show human-readable text
      rotate: this.barcode.rotate,
      textxalign: 'center',      // Always good to set this
    }, function (err, cvs) {
      if (err) {
        //document.getElementById(this.idErr).innerText = 'Error occured. See browser log for more information';
        console.log(err);
      } else {
        //document.getElementById('myimg').setAttribute('src', canvas.toDataURL('image/png'));
      }
    })
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

  private GenerateId(): string {
    let result = '';
    let chars = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
    for (var i = 10; i > 0; --i) result += chars[Math.floor(Math.random() * chars.length)];
    return result;
  }
}
