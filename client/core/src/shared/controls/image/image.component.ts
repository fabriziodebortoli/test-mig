import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryImageSize } from 'ngx-gallery';

import { TbComponentService } from "./../../../core/services/tbcomponent.service";
import { LayoutService } from "./../../../core/services/layout.service";
import { InfoService } from "./../../../core/services/info.service";

import { ControlComponent } from "./../control.component";

@Component({
  selector: "tb-image",
  templateUrl: "./image.component.html",
  styleUrls: ["./image.component.scss"]
})
export class ImageComponent extends ControlComponent {
  @Input() title: string = "";

  @Input() title: string = '';

  imgStyles = {};

  images: NgxGalleryImage[] = [];
  options: NgxGalleryOptions[] = [{
    closeIcon: 'm4-icon m4-tb-closewindows-2',
    image: false,
    thumbnailsColumns: 1,
    previewCloseOnEsc: true,
    previewCloseOnClick: true,
    thumbnailsMargin: 0,
    thumbnailMargin: 0
  }];

  constructor(
    private infoService: InfoService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  getStyles() {

    if (this.width) this.imgStyles['max-width'] = this.width + 'px';

    if (this.height) this.imgStyles['max-height'] = this.height + 'px';

    return this.imgStyles;
  }

  getImages(namespace: string) {
    if (!namespace) return [];

    return [
      {
        small: this.getImageUrl(namespace),
        big: this.getImageUrl(namespace),
        url: this.getImageUrl(namespace)
      }
    ];
  }

  // getImage(){
  //   return ["http://localhost:5000/tbloader/api/tb/document/getImage/?src=Image.TBF.TBFModule.images.SplashConsole_90.jpg"];
  //   // return [this.getImageUrl(this.model.value)];
  // }

  getImageUrl(namespace: string): string {
    return this.infoService.getUrlImage(namespace);
  }
}
