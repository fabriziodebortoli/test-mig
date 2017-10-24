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
    //this.DrawBarcode(scale[0], scale[1]);
  }

  CreateBarcode(): number[] {

    let Width: number;
    let Height: number;

    if (this.IfBarcodeSquared(this.barcode.type)) {
      Width = Height = Math.min((this.rect.rect.right - this.rect.rect.left), (this.rect.rect.bottom - this.rect.rect.top));
    }
    else {
      Width = this.rect.rect.right - this.rect.rect.left;
      Height = this.rect.rect.bottom - this.rect.rect.top;
    }

    let sc: number[] = [1, 1];
    let Id=this.id;
    bwipjs(this.id, {
      bcid: this.barcode.type,       // Barcode type
      text: this.value,	             // Text to encode
      height: (Height * 25.4) / 95.4,
      width: (Width * 25.4) / 95.4,
      scale: 1,
      includetext: this.barcode.type=='ean13'? this.barcode.includetext:false,//this.barcode.includetext,        // Show human-readable text
      rotate: this.barcode.rotate,
      textxalign: 'center',      // Always good to set this
    }, function (err, cvs) {
      if (err) {
        console.log(err);     
      } else {
      }
    });


    return sc;
  }
  
  IncludeText(){
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

  /* DrawBarcode(scX: number, scY: number) {
 
     if (this.IfBarcodeSquared(this.barcode.type)) {
       bwipjs(this.id, {
         bcid: this.barcode.type,       // Barcode type
         text: this.value,//this.value,   	             // Text to encode
         scale: scX,  // scaling
         //scaleY: this.GetInitialScale(this.barcode.type)==0? 1:scY,                 // scaling
 
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
       });
     }
 
     else if (this.barcode.type == 'pdf417') {
       bwipjs(this.id, {
         bcid: this.barcode.type,       // Barcode type
         text: this.value,//this.value,   	             // Text to encode
         scaleX: scX,  // scaling
         scaleY: scY,           // scaling
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
       });
     }
     else {
       bwipjs(this.id, {
         bcid: this.barcode.type,       // Barcode type
         text: this.value,//this.value,   	             // Text to encode
         scaleX: scX,  // scaling
         scaleY: scY,  // scaling
         includetext: this.barcode.includetext,        // Show human-readable text
         rotate: this.barcode.rotate,
         textxalign: 'center',      // Always good to set this
       }, function (err, cvs) {
         if (err) {
           //document.getElementById(this.idErr).innerText = 'Error occured. See browser log for more information';
           let k=1;
           console.log(err);
         } else {
           //document.getElementById('myimg').setAttribute('src', canvas.toDataURL('image/png'));
         }
       });
     }
   }*/

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
