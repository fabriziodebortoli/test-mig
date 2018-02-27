import { ReportingStudioService } from './../../../reporting-studio.service';
import { graphrect } from './../../../models/graphrect.model';
import { Component, Input, ViewChild, ElementRef } from '@angular/core';
import { InfoService } from '@taskbuilder/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';

@Component({
  selector: 'rs-image',
  templateUrl: './image.component.html',
  styles: []
})
export class ReportImageComponent {

  @Input() image: graphrect;
  @ViewChild('rsInnerImg') rsInnerImg: ElementRef;

  constructor(private infoService: InfoService, private httpClient: HttpClient) {
  };

  private loadImage(url: string): Observable<any> {
    return this.httpClient.get(url, { responseType: 'blob'})  // load the image as a blob
  }

  applyStyle(): any {
    let obj = {
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
      'box-shadow': this.image.shadow_height + 'px ' + this.image.shadow_height + 'px ' + this.image.shadow_height + 'px ' + this.image.shadow_color
    };

    const newSrc = this.infoService.getBaseUrl() + '/rs/image/' + this.image.value
    if (this.image.value !== '' && newSrc !== this.image.src) {
        this.image.src = newSrc;
        this.loadImage(this.image.src).subscribe(blob => {
          let reader = new FileReader();
          reader.readAsDataURL(blob);
          reader.onloadend = () => {
              this.rsInnerImg.nativeElement.src = reader.result;
            };
      });
    }
    return obj;
  }

  applyImageStyle(): any {
    let obj = {
      'position': 'relative',
      'max-width': '100%',
      'max-height': '100%'
    };
    return obj;
  }

}
