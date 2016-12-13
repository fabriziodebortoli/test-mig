import { HttpMenuService } from './../../../services/http-menu.service';
import { EventManagerService } from './../../../services/event-manager.service';
import { SettingsService } from './../../../services/settings.service';
import { Logger } from 'libclient';
import { MenuService } from './../../../services/menu.service';
import { ImageService } from './../../../services/image.service';
import { LocalizationService } from './../../../services/localization.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-hidden-tiles',
  templateUrl: './hidden-tiles.component.html',
  styleUrls: ['./hidden-tiles.component.css']
})
export class HiddenTilesComponent implements OnInit {



  constructor(

    private menuService: MenuService,
    private imageService: ImageService,
    private settingsService: SettingsService,
    private localizationService: LocalizationService,
    private eventManagerService: EventManagerService,
    private httpMenuService: HttpMenuService,
    private logger: Logger) {

  }

  ngOnInit() {
    this.eventManagerService.tileHidden.subscribe(tile => {
      this.addToHiddenTiles(tile);
    })
  }

  //---------------------------------------------------------------------------------------------

  addToHiddenTiles(tile) {

    this.httpMenuService.addToHiddenTiles(tile, this.menuService.selectedApplication.name, this.menuService.selectedGroup.name, this.menuService.selectedMenu.name).subscribe(
      result => {
        tile.currentApp = this.menuService.selectedApplication.name;
        tile.currentGroup = this.menuService.selectedGroup.name;
        tile.currentMenu = this.menuService.selectedMenu.name;

        tile.currentAppTitle = this.menuService.selectedApplication.title;
        tile.currentGroupTitle = this.menuService.selectedGroup.title;
        tile.currentMenuTitle = this.menuService.selectedMenu.title;

        this.addToHiddenTilesArray(tile);
      });
  }

  //---------------------------------------------------------------------------------------------
  removeFromHiddenTiles(tile) {

    this.httpMenuService.removeFromHiddenTiles(tile, this.menuService.selectedApplication.name, this.menuService.selectedGroup.name, this.menuService.selectedMenu.name).subscribe(
      result => {
        this.removeFromHiddenTilesArray(tile);
      });
  }
  //---------------------------------------------------------------------------------------------
  addToHiddenTilesArray(tile) {

    tile.hiddenTile = true;

    for (var i = 0; i < this.menuService.hiddenTiles.length; i++) {
      if (this.menuService.hiddenTiles[i] == tile) {
        return;
      }
    }

    this.menuService.hiddenTiles.push(tile);
    this.menuService.hiddenTilesCount++;

    this.showOthers();
  };

  //---------------------------------------------------------------------------------------------
  removeFromHiddenTilesArray(tile) {
    var index = -1;

    for (var i = 0; i < this.menuService.hiddenTiles.length; i++) {
      if (this.menuService.hiddenTiles[i] == tile) {
        index = i;
        break;
      }
    }
    if (index >= 0) {
      tile.hiddenTile = false;
      this.menuService.hiddenTiles.splice(index, 1);
      this.menuService.hiddenTilesCount--;
    }
  };


  //---------------------------------------------------------------------------------------------
  getTileTooltip(tile) {
    //   tile.tileTooltip = $sce.trustAsHtml(
    //  this.localizationService.getLocalizedElement('ApplicationLabel') + ": " + tile.currentAppTitle + "<br/>" +
    //   this.localizationService.getLocalizedElement('ModuleLabel') + ": " + tile.currentGroupTitle + "<br/>" +
    //   this.localizationService.getLocalizedElement('MenuLabel') + ": " + tile.currentMenuTitle);
  };

  getCurrentMenuHiddenTiles()
  {
    let array = []

    for (var i = 0; i < this.menuService.hiddenTiles.length; i++) {
      if (this.menuService.hiddenTiles[i].currentMenuTitle == this.menuService.selectedMenu.title)
        array.push(this.menuService.hiddenTiles[i]);
    }

    return array;
  }

  getOtherMenuHiddenTiles(){
   let array = []

    for (var i = 0; i < this.menuService.hiddenTiles.length; i++) {
      if (this.menuService.hiddenTiles[i].currentMenuTitle != this.menuService.selectedMenu.title)
        array.push(this.menuService.hiddenTiles[i]);
    }

    return array;

  }
  
  /*controlla se ci sono dei tile nascosti nel menu corrente*/
  //---------------------------------------------------------------------------------------------
  ifMenuExistInHiddenTiles  () {

    if (this.menuService.selectedMenu == undefined || this.menuService.selectedApplication == undefined)
      return false;

    for (var i = 0; i < this.menuService.hiddenTiles.length; i++) {
      if ((this.menuService.hiddenTiles[i].currentMenuTitle == this.menuService.selectedMenu.title) && (this.menuService.hiddenTiles[i].currentAppTitle == this.menuService.selectedApplication.title))
        return true;
    }
    return false;
  }

  //---------------------------------------------------------------------------------------------
  ifOtherTilesAreHidden  () {
    if (this.menuService.selectedMenu == undefined)
      return true;

    for (var i = 0; i < this.menuService.hiddenTiles.length; i++) {
      if (this.menuService.hiddenTiles[i].currentMenuTitle != this.menuService.selectedMenu.title)
        return true;
    }
    return false;
  }

  //---------------------------------------------------------------------------------------------

  showOthers() {
    // var display = $(".othersHiddenContainer").css("display");
    // if (display == 'none')
    //   $(".othersHiddenContainer").css("display", "block");

  }


  //---------------------------------------------------------------------------------------------

  hideOthers() {
    // var display = $(".othersHiddenContainer").css("display");
    // if (display == 'none')
    //   $(".othersHiddenContainer").css("display", "block");
    // else
    //   $(".othersHiddenContainer").css("display", "none");

  }
}
