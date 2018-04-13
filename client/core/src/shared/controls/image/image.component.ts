import { Component, Input, ChangeDetectorRef } from '@angular/core';
// import { NgxGalleryOptions, NgxGalleryImage } from 'ngx-gallery';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { InfoService } from './../../../core/services/info.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-image',
  templateUrl: './image.component.html',
  styleUrls: ['./image.component.scss']
})
export class ImageComponent extends ControlComponent {

  @Input() title: string = '';

  // images: NgxGalleryImage[];
  // options: NgxGalleryOptions[] = [{
  //   image: false,
  //   height: '100px'
  // }, {
  //   breakpoint: 500,
  //   width: '100%'
  // }];

  constructor(
    public infoService: InfoService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  getStyles() {

    let imgStyles = {};

    if (this.width) imgStyles['max-width'] = this.width + 'px';
    if (this.height) imgStyles['max-height'] = this.height + 'px';


    // this.options = [{
    //   // image: false,
    //   // height: '100px'
    // }, {
    //   breakpoint: 500,
    //   width: '100%'
    // }];

    return imgStyles;
  }

  // getImage(){
  //   return ["http://localhost:5000/tbloader/api/tb/document/getImage/?src=Image.TBF.TBFModule.images.SplashConsole_90.jpg"];
  //   // return [this.getImageUrl(this.model.value)];
  // }

  getImageUrl(namespace: string) {
    return this.infoService.getDocumentBaseUrl() + 'getImage/?src=' + namespace;
  }

}
