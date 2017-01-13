import { ImageService } from './../../services/image.service';
import { LocalizationService } from './../../services/localization.service';
import { MenuService } from './../../services/menu.service';
import { EventManagerService } from './../../services/event-manager.service';
import { SettingsService } from './../../services/settings.service';
import { HttpMenuService } from './../../services/http-menu.service';
import { UtilsService, WebSocketService } from 'tb-core';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css'],
})

export class MenuComponent implements OnInit {

  constructor(
    private webSocketService: WebSocketService,
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private settingsService: SettingsService,
    private localizationService: LocalizationService,
    private eventManagerService: EventManagerService
  ) {

  }
  ngOnInit() {

  }

  runDocument(ns: string) {
    this.webSocketService.runObject(ns);
  }
}
