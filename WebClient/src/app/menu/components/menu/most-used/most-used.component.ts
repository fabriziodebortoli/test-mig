import { Component, Input, OnInit } from '@angular/core';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
import { LocalizationService } from './../../../services/localization.service';

@Component({
  selector: 'tb-most-used',
  templateUrl: './most-used.component.html',
  styleUrls: ['./most-used.component.css']
})
export class MostUsedComponent implements OnInit {
  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService, 
    private localizationService: LocalizationService
  ) {
  }
  ngOnInit() {
  }

  clearAll() {

    this.httpMenuService.mostUsedClearAll().subscribe(result => {
    this.menuService.clearMostUsed();
    });

  }
}