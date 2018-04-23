import { ReportSnapshotDropdownComponent } from './report-snapshot-dropdown/report-snapshot-dropdown.component';
import { Component, Input, ViewEncapsulation, TemplateRef, ViewChild } from '@angular/core';

import { UtilsService, SettingsService } from '@taskbuilder/core';

import { ItemCustomizationsDropdownComponent } from './item-customizations-dropdown/item-customizations-dropdown.component';

import { ImageService } from './../../../services/image.service';
import { EasystudioService } from './../../../services/easystudio.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { RsSnapshotService } from '@taskbuilder/core';
@Component({
  selector: 'tb-menu-element',
  templateUrl: './menu-element.component.html',
  styleUrls: ['./menu-element.component.scss']
})
export class MenuElementComponent {

  @Input() object: any;
  @Input() showEasyBuilderOptions: boolean = true;
  @Input() showLongTooltip: boolean = false;

  @ViewChild('snapshot', { read: ReportSnapshotDropdownComponent }) public snapshot: ReportSnapshotDropdownComponent;

  lorem: string = 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam';

  constructor(
    public httpMenuService: HttpMenuService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public settingService: SettingsService,
    public easystudioService: EasystudioService,
    public imageService: ImageService,
    public rsSnapshotService: RsSnapshotService
  ) {
    this.lorem = this.lorem.slice(0, Math.floor((Math.random() * 147) + 55));
  }

  public snapshotsCount: number = 0;

  //---------------------------------------------------------------------------------------------
  getFavoriteClass(object) {
    return object.isFavorite ? 'tb-favoritespin' : 'tb-favorites';
  }

  initAndRunSnapshots(object: any) {
    var objType = object.objectType.toLowerCase();
    if (objType != 'report') {
      this.runFunction(object);
      return;
    }

    this.rsSnapshotService.getSnapshotData(object.target).subscribe(resp => {
      if (resp.length > 0) {
        this.snapshot.togglePopup();
      }
      else
        this.runFunction(object);
    });
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
