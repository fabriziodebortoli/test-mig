import { ReportingStudioService } from './../../../../reporting-studio/reporting-studio.service';
import { WebSocketService } from './../../../../core/websocket.service';
import { Component, Input } from '@angular/core';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';


@Component({
  selector: 'tb-tile-element',
  templateUrl: './tile-element.component.html',
  styleUrls: ['./tile-element.component.css']
})
export class TileElementComponent {

  private showFavorites: boolean = true;

  private object: any;
  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService,
    private webSocketService: WebSocketService,
    private rsService: ReportingStudioService
  ) {
    this.webSocketService.windowOpen.subscribe(data => {
      this.object.isLoading = false;
    });

    this.rsService.reportOpened.subscribe(arg=>
    {
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

  runFunction(object)
  {
    this.menuService.runFunction(object); 
    object.isLoading = true;
  }
}
