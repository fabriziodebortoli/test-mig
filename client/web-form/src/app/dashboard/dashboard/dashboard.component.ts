import { LocalizationService } from '@taskbuilder/core';
import { SettingsService } from '@taskbuilder/core';
import { ImageService } from '@taskbuilder/core';
import { MenuService } from '@taskbuilder/core';
import { UtilsService } from '@taskbuilder/core';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  private favorites: Array<any> = [];

  constructor(
    private menuService: MenuService,
    private imageService: ImageService,
    private utilsService: UtilsService,
    private settingsService: SettingsService,
    private localizationService: LocalizationService

  ) { }

  ngOnInit() {
  }

  runFunction(object) {
    this.menuService.runFunction(object);
  }

}
