import { Component, Input, ViewEncapsulation } from '@angular/core';

import { HttpMenuService, MenuService, UtilsService, SettingsService, EasystudioService, ImageService } from '@taskbuilder/core';

import { ItemCustomizationsDropdownComponent } from './item-customizations-dropdown/item-customizations-dropdown.component';

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
    public easystudioService: EasystudioService,
    public imageService: ImageService
  ) {
    this.lorem = this.lorem.slice(0, Math.floor((Math.random() * 147) + 55));
  }

  //---------------------------------------------------------------------------------------------
  getFavoriteClass(object) {
    return object.isFavorite ? 'tb-favoritespin' : 'tb-favorites';
  }

  //---------------------------------------------------------------------------------------------
  runFunction(object) {
    event.stopPropagation();
    this.menuService.runFunction(object);
  }

  //---------------------------------------------------------------------------------------------
  canShowEasyStudioButton(object) {
    return this.showEasyBuilderOptions &&
      this.easystudioService.easyStudioActivation() &&
      (object.objectType.toLowerCase() == 'document' || object.objectType.toLowerCase() == 'batch') &&
      !object.noeasystudio;
  }


}
