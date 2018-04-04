import { TbComponent, UtilsService, EventManagerService, TbComponentService } from '@taskbuilder/core';
import { Component, Input, HostBinding, ChangeDetectorRef } from '@angular/core';

import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';

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
    this.objects = this._tile.Object.filter(element => {
      let env = element.environment ? element.environment.toLowerCase() : '';
      let show = env == '' || (this.tbComponentService.infoService.isDesktop && env == 'desktop') || (!this.tbComponentService.infoService.isDesktop && env == 'web')
      return show;
    });
  }

  getPinnedClass(tile) {
    return tile.pinned ? 'hdr_strong' : 'hdr_weak';
  }
}