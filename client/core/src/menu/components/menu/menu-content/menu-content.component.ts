import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { TbComponent } from './../../../../shared/components/tb.component';
import { EventManagerService } from './../../../../core/services/event-manager.service';
import { Component, Input, HostBinding, ChangeDetectorRef } from '@angular/core';

import { ImageService } from './../../../services/image.service';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';

@Component({
  selector: 'tb-menu-content',
  templateUrl: './menu-content.component.html',
  styleUrls: ['./menu-content.component.scss']
})
export class MenuContentComponent extends TbComponent {

  @HostBinding('class.brick--width2') width2: boolean = false;


  constructor(
    public httpMenuService: HttpMenuService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public imageService: ImageService,
    public eventManagerService: EventManagerService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();

  }
  pinned: boolean = false;
  public objects: any;
  public _tile: any;

  @Input()
  public canHide: boolean = true;

  @Input() menu: any;

  @Input()
  get tile(): any {
    return this._tile;
  }

  set tile(tile: any) {
    this._tile = tile;
    this.objects = this._tile.Object.filter(x =>  x.noweb != undefined ? !x.noweb : true);
  }

  getPinnedClass(tile) {
    return tile.pinned ? 'hdr_strong' : 'hdr_weak';
  }
}