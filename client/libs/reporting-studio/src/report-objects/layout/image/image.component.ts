import { RsExportService } from './../../../rs-export.service';
import { rect } from './../../../models/rect.model';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { graphrect } from './../../../models/graphrect.model';
import { Component, Input, ViewChild, ElementRef } from '@angular/core';
import { InfoService, UtilsService } from '@taskbuilder/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { ImgFitMode } from './../../../models/image-fit-mode.model';
import { PdfType, SvgType, PngType } from './../../../models/export-type.model';

@Component({
  selector: 'rs-image',
  templateUrl: './image.component.html',
  styles: []
})
export class ReportImageComponent {

  @Input() image: graphrect;
  @ViewChild('rsInnerImg') rsInnerImg: ElementRef;

  constructor(private infoService: InfoService, private httpClient: HttpClient, public utils: UtilsService,
    public rsExportService: RsExportService) {
  };

  private loadImage(url: string): Observable<any> {   
    return this.httpClient.get(url, { responseType: 'blob'})  // load the image as a blob
  }

  applyStyle(): any {
    let rgba = this.utils.hexToRgba(this.image.bkgcolor);
    rgba.a = this.image.transparent ? 0 : 1;
    let backgroundCol = 'rgba(' + rgba.r + ',' + rgba.g + ',' + rgba.b + ',' + rgba.a + ')';
    let obj = {
      'background-color': backgroundCol,
      'position': 'absolute',
      'left': this.image.rect.left + 'px',
      'top': this.image.rect.top + 'px',
      'width': this.image.rect.right - this.image.rect.left + 'px',
      'height': this.image.rect.bottom - this.image.rect.top + 'px',
      'border-left': this.image.borders.left ? this.image.pen.width + 'px' : '0px',
      'border-right': this.image.borders.right ? this.image.pen.width + 'px' : '0px',
      'border-bottom': this.image.borders.bottom ? this.image.pen.width + 'px' : '0px',
      'border-top': this.image.borders.top ? this.image.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'border-color': this.image.pen.color,
      'border-radius': this.image.ratio + 'px',
      'box-sizing': 'border-box',
      'box-shadow': this.image.shadow_height + 'px ' + this.image.shadow_height + 'px ' + this.image.shadow_height + 'px ' + this.image.shadow_color
    };

    const newSrc = this.infoService.getBaseUrl() + '/rs/image/' + this.image.value
    if (this.image.value !== '' && newSrc !== this.image.src) {
        this.image.src = newSrc;
        if(this.rsExportService.pdfState === PdfType.PDF)
          this.rsExportService.incrementImgCounter();
        this.loadImage(this.image.src).subscribe(blob => {
          let reader = new FileReader();
          reader.readAsDataURL(blob);
          reader.onloadend = () => {
              this.rsInnerImg.nativeElement.src = reader.result;
              if(this.rsExportService.pdfState === PdfType.PDF) {
                this.rsInnerImg.nativeElement.onload = () => {
                  this.rsExportService.decrementImgCounter();
              };
            }
          };
      });
    }
    return obj;
  }

  applyImageStyle(): any {
    let imgIsPortrait;
    let imgRatioWH;
    if(this.rsInnerImg && this.rsInnerImg.nativeElement)
      imgRatioWH =  this.rsInnerImg.nativeElement.naturalWidth / this.rsInnerImg.nativeElement.naturalHeight;
      imgIsPortrait = imgRatioWH < 1;
    
    let recRatioWH =  (this.image.rect.right - this.image.rect.left) / (this.image.rect.bottom - this.image.rect.top);
    let recIsPortrait = recRatioWH < 1; 

    let height = 'fit-content';
    let width = 'fit-content';   

    if(this.image.fit_mode == ImgFitMode.BEST)
    {
      if ((imgIsPortrait && !recIsPortrait)
      || (imgIsPortrait == recIsPortrait && imgRatioWH < recRatioWH))
      {
        height = 'inherit';
        width = 'initial';
      }
        
     if ((!imgIsPortrait && recIsPortrait)
      || (imgIsPortrait === recIsPortrait && imgRatioWH > recRatioWH))
      {
          height = 'initial';
          width = 'inherit';
      }
    }
    else if (this.image.fit_mode === ImgFitMode.STRETCH)
      {       
          height = 'inherit';        
          width = 'inherit';
      }
      if (this.image.vertical_align === 'middle')
        height = 'inherit';
      if (this.image.text_align === 'center')
        width = 'inherit';


      let obj = {
      'object-fit': this.image.fit_mode === ImgFitMode.BEST ? 'contain' : this.image.fit_mode === ImgFitMode.ORIGINAL ? 'none' : 'fill',
      'position': 'absolute',
      'width': width,
      'height': height,
      'left': this.image.text_align !== 'right' ? '0px' : 'unset',
      'right': this.image.text_align === 'right'? '0px':'unset',
      'top': this.image.vertical_align !== 'bottom'? '0px':'unset',
      'bottom': this.image.vertical_align === 'bottom'? '0px':'unset',
      'display':'inline-block',
      'max-width': '100%',
      'max-height': '100%'
    };

    return obj;
  }

}
