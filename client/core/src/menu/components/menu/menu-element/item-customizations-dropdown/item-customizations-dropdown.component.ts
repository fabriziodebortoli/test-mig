import { LocalizationService } from './../../../../../core/services/localization.service';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { EasyStudioContextComponent } from './../../../../../shared/components/easystudio-context/easystudio-context.component';
import { SettingsService } from './../../../../services/settings.service';
import { Component, Input, ViewEncapsulation, OnDestroy, TemplateRef, ElementRef, HostListener, ViewChild, OnInit } from '@angular/core';
import { HttpMenuService } from './../../../../services/http-menu.service';
import { PopupRef, PopupService } from '@progress/kendo-angular-popup';

interface MyCust {
  fileName: string
  customizationName: string
  applicationOwner: string
  moduleOwner: string
}
@Component({
  selector: 'tb-item-customizations-dropdown',
  templateUrl: './item-customizations-dropdown.component.html',
  styleUrls: ['./item-customizations-dropdown.component.scss'],
  encapsulation: ViewEncapsulation.None
})

export class ItemCustomizationsDropdownComponent implements OnDestroy, OnInit {
  public isDesignable: boolean = false;
  public openContextMenu: boolean = false;
  private popupRef: PopupRef;

  public localizationsLoadedSubscription: any;
  public customizationsLoadedSubscription: any;
  public localizationLoaded: boolean;
  public currentModule: any;
  public currentApplication: any;
  public customizations: MyCust[];
  @Input() objectM: any;
  public memory: { Customizations: MyCust[] };
  elRef: HTMLElement;
  offsetLeft: any;
  offsetTop: any;


  constructor(elRef: ElementRef,
    public httpMenuService: HttpMenuService,
    public localizationService: LocalizationService,
    public popupService: PopupService,
    public settingService: SettingsService) {

    this.elRef = elRef.nativeElement;
  }
  //--------------------------------------------------------------------------------
  ngOnInit(): void {
    let sub = this.httpMenuService.getCurrentContext().subscribe((result) => {
      this.extractNames(result);
      sub.unsubscribe();
    });
    this.localizationsLoadedSubscription = this.localizationService.localizationsLoaded.subscribe((loaded) => {
      this.localizationLoaded = loaded;
    });
    let sub2 = this.httpMenuService.initEasyStudioData(this.objectM).subscribe((result) => {
      this.memory = { Customizations: [] };
      let res = result["_body"];
      if (res !== "") {
        this.memory = JSON.parse(result["_body"]);
        if (this.memory != undefined) {
          this.customizations = [];
          let r = this.memory.Customizations;
          r.forEach(element => {
            this.customizations.push(element);
          });
        }
      }
      sub2.unsubscribe();
    });

  }

  //--------------------------------------------------------------------------------
  ngOnDestroy() {
    this.localizationsLoadedSubscription.unsubscribe();
  }

  //--------------------------------------------------------------------------------
  close() {
    if (this.popupRef) {
      this.popupRef.close();
      this.popupRef = null;
    }
  }

    //---------------------------------------------------------------------------------------------
    getCustomizClass(customization: MyCust) {
      return this.isCustomizationEnabled(customization) ? 'is-enabled' : 'is-disabled';
    }

  //--------------------------------------------------------------------------------
  initEasyStudio(object, template: TemplateRef<any>) {
    this.isDesignable = this.customizations != undefined;
    this.togglePopup(template);
  }

  //--------------------------------------------------------------------------------
  public togglePopup(template: TemplateRef<any>) {
    this.offsetLeft = this.elRef.getBoundingClientRect().left;
    this.offsetTop = this.elRef.getBoundingClientRect().top + 15;
    if (this.popupRef) {
      this.popupRef.close();
      this.popupRef = null;
    } else {
      this.popupRef = this.popupService.open({
        content: template,
        offset: { top: this.offsetTop, left: this.offsetLeft },
        anchorAlign: { horizontal: 'right', vertical: 'bottom' },
        popupAlign: { horizontal: 'right', vertical: 'top' }

      });
    }
  }

  //--------------------------------------------------------------------------------
  private extractNames(result: Response) {
    if (result == undefined) return;
    let res = result["_body"];
    if (res !== "") {
      let array: string[] = res.toString().split(';');
      if (array.length != 2) return;
      this.currentApplication = array[0];
      this.currentModule = array[1];
    }
  }
  //--------------------------------------------------------------------------------
  isEasyStudioDocument(object) {
    let sub = this.httpMenuService.isEasyStudioDocument(object).subscribe((result) => {
      return result;
    });
    sub.unsubscribe();
  }

  //--------------------------------------------------------------------------------
  getCustomizationTooltip(customization) {
    //TODOROBY
  }

  //--------------------------------------------------------------------------------  
  isContextActive() {
    return this.currentApplication !== undefined && this.currentModule !== undefined;
   }

  //--------------------------------------------------------------------------------  
  isCustomizationEnabled(customization) {
    if (!this.isContextActive())
      return false;
    return this.currentApplication === customization.applicationOwner && this.currentModule === customization.moduleOwner;
  }

  //--------------------------------------------------------------------------------
  openEasyStudioAndContextIfNeeded(object, customization: MyCust) {
    // if (!(this.currentApplication !== undefined && this.currentApplication !== null && this.currentModule !== undefined && this.currentModule !== undefined)) {
    //   this.openContextMenu = true;
    //   let sub = this.httpMenuService.runEasyStudio(object.target, object.customizationName).subscribe((result) => {
    //     alert("easyStudio lanciato");
    //     sub.unsubscribe();
    //   });
    // }
    // else {}
    if(customization !== null && !this.isCustomizationEnabled(customization))
      return;
      let custName = customization !== null ? customization.customizationName : undefined;
    let sub = this.httpMenuService.runEasyStudio(object.target, custName).subscribe((result) => {
      this.close();
      sub.unsubscribe();
    });

  }
  //--------------------------------------------------------------------------------
  cloneAsEasyStudioDocumentIfNeeded(object) {
    // if (!(this.currentApplication !== undefined && this.currentApplication !== null && this.currentModule !== undefined && this.currentModule !== undefined)) {
    //   this.openContextMenu = true;
    //   let sub = this.httpMenuService.cloneAsEasyStudioDocument(object).subscribe((result) => {
    //     alert("clone lanciato");
    //     sub.unsubscribe();
    //   });
    // }
    // else {
    //   let sub = this.httpMenuService.cloneAsEasyStudioDocument(object).subscribe((result) => {
    //     alert("clone lanciato");
    //     sub.unsubscribe();
    //   });
    // }
  }

}
