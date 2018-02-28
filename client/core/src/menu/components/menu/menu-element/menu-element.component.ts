import { SettingsService } from './../../../../core/services/settings.service';
import { ItemCustomizationsDropdownComponent } from './item-customizations-dropdown/item-customizations-dropdown.component';
import { Component, Input, ViewEncapsulation } from '@angular/core';
import { ImageService } from './../../../services/image.service';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';

@Component({
  selector: 'tb-menu-element',
  templateUrl: './menu-element.component.html',
  styleUrls: ['./menu-element.component.scss']
})
export class MenuElementComponent {

  @Input() object: any;
  @Input() showEasyBuilderOptions: boolean = true;
  @Input() showLongTooltip: boolean = false;

  lorem: string = 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam';

  constructor(
    public httpMenuService: HttpMenuService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public settingService: SettingsService,
    public imageService: ImageService
  ) {
    this.lorem = this.lorem.slice(0, Math.floor((Math.random() * 147) + 55));
  }

  //---------------------------------------------------------------------------------------------
  getFavoriteClass(object) {
    return object.isFavorite ? 'tb-filledstar' : 'tb-emptystar';
  }

  //---------------------------------------------------------------------------------------------
  runFunction(object) {
    event.stopPropagation();
    this.menuService.runFunction(object);
  }

  //---------------------------------------------------------------------------------------------
  canShowEasyStudioButton(object) {
    return this.showEasyBuilderOptions &&
      this.settingService.IsEasyStudioActivated &&
      (object.objectType.toLowerCase() == 'document' || object.objectType.toLowerCase() == 'batch') &&
      !object.noeasystudio;
  }


}
