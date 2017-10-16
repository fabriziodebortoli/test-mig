import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { EasyStudioContextComponent } from './../../../../../shared/components/easystudio-context/easystudio-context.component';
import { LocalizationService } from './../../../../services/localization.service';
import { SettingsService } from './../../../../services/settings.service';
import { Component, Input, ViewEncapsulation, OnDestroy, TemplateRef, ElementRef, HostListener, ViewChild, OnInit } from '@angular/core';
import { HttpMenuService } from './../../../../services/http-menu.service';
import { PopupRef, PopupService } from '@progress/kendo-angular-popup';

interface MyObj {
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
  public customizations: string[];
  @Input() objectM: any;
  //public show: boolean = false;
  public memory: { Customizations: MyObj[] };
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
            this.customizations.push(element.customizationName);
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
  initEasyStudioData(object, template: TemplateRef<any>, ref: ElementRef) {
   /* let sub = this.httpMenuService.initEasyStudioData(object).subscribe((result) => {
      this.memory = { Customizations: [] };
      let res = result["_body"];
      if (res !== "") {
        this.memory = JSON.parse(result["_body"]);
        if (this.memory != undefined) {
          this.customizations = [];
          let r = this.memory.Customizations;
          r.forEach(element => {
            this.customizations.push(element.customizationName);
          });
        }
      }*/
      this.isDesignable = this.customizations != undefined;
   //   this.onToggle();
      this.togglePopup(template, ref);
      //sub.unsubscribe();
   // });

  }

  //--------------------------------------------------------------------------------
  public togglePopup(template: TemplateRef<any>, ref: ElementRef) {
    this.offsetLeft = this.elRef.getBoundingClientRect().left;
    this.offsetTop = this.elRef.getBoundingClientRect().top;
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
    let array: string[] = result.toString().split(';');
    if (array.length != 2) return;
    this.currentApplication = array[0];
    this.currentModule = array[1];
  }
  //--------------------------------------------------------------------------------
  isEasyStudioDocument(object) {
    let sub = this.httpMenuService.isEasyStudioDocument(object).subscribe((result) => {
      return result;
    });
    sub.unsubscribe();
  }


/*
  //--------------------------------------------------------------------------------
  public onToggle(): void {
    this.show = !this.show;
  }

  //--------------------------------------------------------------------------------
  public close() {
    this.show = false;
  }*/

  //--------------------------------------------------------------------------------
  getCustomizationTooltip(customization) {

  }

  //--------------------------------------------------------------------------------  
  isCustomizationEnabled(customization) {
    if (this.currentApplication == undefined || this.currentModule == undefined)
      return false;

    var appAndModulePartialPath = "\\" + this.currentApplication + "\\" + this.currentModule + "\\";
    var ind = customization.fileName.toLowerCase().indexOf(appAndModulePartialPath.toLowerCase());

    return ind > 0;
  }

  //--------------------------------------------------------------------------------
  openEasyStudioAndContextIfNeeded(object) {
    // if (!(this.currentApplication !== undefined && this.currentApplication !== null && this.currentModule !== undefined && this.currentModule !== undefined)) {
    //   this.openContextMenu = true;
    //   let sub = this.httpMenuService.runEasyStudio(object.target, object.customizationName).subscribe((result) => {
    //     alert("easyStudio lanciato");
    //     sub.unsubscribe();
    //   });
    // }
    // else {}
    let sub = this.httpMenuService.runEasyStudio(object.target, object.customizationName).subscribe((result) => {
      alert("easyStudio lanciato");
     // this.close();
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
    alert("clone lanciato");
  }


}
