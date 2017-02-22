import { Component, Input } from '@angular/core';
import { UtilsService, WebSocketService, EventDataService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';


@Component({
  selector: 'tb-tile-element',
  templateUrl: './tile-element.component.html',
  styleUrls: ['./tile-element.component.css']
})
export class TileElementComponent {

  private object: any;
  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService,
    private webSocketService: WebSocketService,
    private eventData: EventDataService
  ) {
    this.webSocketService.windowOpen.subscribe(data => {
      this.object.isLoading = false;
    });

    this.eventData.opened.subscribe(arg => {
      this.object.isLoading = false;
    });
  }

  get Object(): any {
    return this.object;
  }

  @Input()
  set Object(object: any) {
    this.object = object;
  }

  getFavoriteClass(object) {
    return object.isFavorite ? 'star' : 'star_border';
  }

  runFunction(object) {
    object.isLoading = true;
    this.menuService.runFunction(object);

  }
}
