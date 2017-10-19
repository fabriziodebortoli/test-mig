import { EsCustomizItem } from './../../../../../shared/models/es-customization-item.model';
import { EasystudioService } from './../../../../../core/services/easystudio.service';
import { LocalizationService } from './../../../../../core/services/localization.service';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { EasyStudioContextComponent } from './../../../../../shared/components/easystudio-context/easystudio-context.component';
import { Component, Input, ViewEncapsulation, OnDestroy, TemplateRef, ElementRef, HostListener, ViewChild, OnInit } from '@angular/core';
import { HttpMenuService } from './../../../../services/http-menu.service';
import { PopupRef, PopupService } from '@progress/kendo-angular-popup';

@Component({
  selector: 'tb-item-customizations-dropdown',
  templateUrl: './item-customizations-dropdown.component.html',
  styleUrls: ['./item-customizations-dropdown.component.scss'],
  encapsulation: ViewEncapsulation.None
})

export class ItemCustomizationsDropdownComponent implements OnDestroy, OnInit {
  private popupRef: PopupRef;

  public localizationsLoadedSubscription: any;
  public localizationLoaded: boolean;


  @Input() objectM: any;

  elRef: HTMLElement;
  offsetLeft: any;
  offsetTop: any;


  constructor(elRef: ElementRef,
    public easystudioService: EasystudioService,
    public localizationService: LocalizationService,
    public popupService: PopupService) {
    this.elRef = elRef.nativeElement;
  }

  //--------------------------------------------------------------------------------
  ngOnInit(): void {
    this.localizationsLoadedSubscription = this.localizationService.localizationsLoaded.subscribe((loaded) => {
      this.localizationLoaded = loaded;
    });
  }

  //--------------------------------------------------------------------------------
  ngOnDestroy() {
    this.localizationsLoadedSubscription.unsubscribe();
    if (this.popupRef)
      this.popupRef.close();

  }

  //--------------------------------------------------------------------------------
  close() {
    if (this.popupRef) {
      this.popupRef.close();
      this.popupRef = null;
    }
  }

  //---------------------------------------------------------------------------------------------
  getNewCustClass() {
    return this.easystudioService.isContextActive() ? 'is-enabled' : 'is-disabled';
  }

  //---------------------------------------------------------------------------------------------
  getCustomizClass(customization: EsCustomizItem) {
    return this.isCustomizationEnabled(customization) ? 'is-enabled' : 'is-disabled';
  }

  //--------------------------------------------------------------------------------
  initEasyStudio(object, template: TemplateRef<any>) {
    this.easystudioService.initEasyStudioData(object);
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
  isEasyStudioDocument(object) {
    return this.easystudioService.isEasyStudioDocument(object);
  }

  //--------------------------------------------------------------------------------
  getCustomizationTooltip(customization) {
    //TODOROBY
  }

  //--------------------------------------------------------------------------------  
  isCustomizationEnabled(customization) {
    if (!this.easystudioService.isContextActive())
      return false;
    return this.easystudioService.currentApplication === customization.applicationOwner && this.easystudioService.currentModule === customization.moduleOwner;
  }

  //--------------------------------------------------------------------------------
  openEasyStudioAndContextIfNeeded(object, customization: EsCustomizItem) {
    // if (!(this.currentApplication !== undefined && this.currentApplication !== null && this.currentModule !== undefined && this.currentModule !== undefined)) {
    //   this.openContextMenu = true;
    //   let sub = this.httpMenuService.runEasyStudio(object.target, object.customizationName).subscribe((result) => {
    //     alert("easyStudio lanciato");
    //     sub.unsubscribe();
    //   });
    // }
    // else {}
    if (customization !== null && !this.isCustomizationEnabled(customization))
      return;
    let custName = customization !== null ? customization.customizationName : undefined;
    this.easystudioService.runEasyStudio(object.target, custName);
    this.close();
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
