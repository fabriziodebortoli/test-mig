import { Component, Input } from '@angular/core';

import { UtilsService } from './../../../../core/services/utils.service';
import { EventManagerService } from './../../../services/event-manager.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';


@Component({
  selector: 'tb-menu-content',
  template: "<masonry-brick class=\"brick\" [ngClass]=\"{'brick--width2': getObjects().length > 10 }\"> <md-card *ngIf=\"tile != undefined\"> <md-card-title> <span>{{tile.title}}</span> </md-card-title> <md-card-content> <div class=\"row\"> <tb-menu-element [object]=\"object\" *ngFor=\"let object of getObjects();\" class=\"menu-content col-xs-12\" [ngClass]=\"{'col-md-6': getObjects().length > 10 , 'col-md-12': getObjects().length <= 10}\"></tb-menu-element> </div> </md-card-content> </md-card> </masonry-brick>",
  styles: [":host(tb-menu-content).limited md-card-content { max-height: 215px; overflow: auto; } /* @custom-media --sm-viewport only screen and (min-width: 48em); @custom-media --md-viewport only screen and (min-width: 64em); @custom-media --lg-viewport only screen and (min-width: 75em); */ .brick { width: 100%; } .brick--width2 { width: 100%; } @media screen and (min-width: 48em) { .brick { width: 50%; } .brick--width2 { width: 100%; } } @media screen and (min-width: 64em) { .brick { width: 33.3%; } .brick--width2 { width: 66.6%; } } @media screen and (min-width: 75em) { .brick { width: 25%; } .brick--width2 { width: 50%; } } tb-menu-element { padding-right: 0.3rem; padding-left: 0.3rem; } "]
})
export class MenuContentComponent {

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService,
    private eventManagerService: EventManagerService
  ) {
  }

  @Input('tile') tile: any;

  getObjects() {
    return this.utilsService.toArray(this.tile.Object);
  }

  getPinnedClass(tile) {
    return tile.pinned ? 'hdr_strong' : 'hdr_weak';
  }
}