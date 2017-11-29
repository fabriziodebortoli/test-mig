import { CloneDocumentDialogComponent } from './../../../../../shared/components/clone-document-dialog/clone-document-dialog.component';
import { EsCustomizItem } from './../../../../../shared/models/es-customization-item.model';
import { EasystudioService } from './../../../../../core/services/easystudio.service';
import { OldLocalizationService } from './../../../../../core/services/oldlocalization.service';
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

  @ViewChild('anchor', { read: ElementRef }) public anchor: ElementRef;
  @ViewChild('template', { read: TemplateRef }) public template: TemplateRef<any>;
  @ViewChild(CloneDocumentDialogComponent) cloneDialog: CloneDocumentDialogComponent;
  @Input() objectM: any;

  elRef: HTMLElement;
  offsetLeft: any;
  offsetTop: any;
  public newName: string;
  public newTitle: string;
  public isNotEasyStudioDocument : boolean;

  @HostListener('window:keyup', ['$event'])
  public keyup(event: KeyboardEvent): void {
    if (event.keyCode === 27) {
      this.close();
    }
  }

  @HostListener('document:click', ['$event'])
  public documentClick(event: any): void {
    if (!this.contains(event.target)) {
      this.close();
    }
  }

  constructor(elRef: ElementRef,
    public easystudioService: EasystudioService,
    public localizationService: OldLocalizationService,
    public popupService: PopupService) {
    this.elRef = elRef.nativeElement;
  }

  openCloneDialog(){
    this.cloneDialog.open();
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
    this.isNotEasyStudioDocument = !this.easystudioService.isEasyStudioDocument(this.objectM);
    this.togglePopup(template);
  }

  //--------------------------------------------------------------------------------
  public togglePopup(template: TemplateRef<any>) {
    this.offsetLeft = this.elRef.getBoundingClientRect().left + 3;
    this.offsetTop = this.elRef.getBoundingClientRect().top - 4;
    if (this.popupRef) {
      this.popupRef.close();
      this.popupRef = null;
    } else {
      this.popupRef = this.popupService.open({
        content: template,
        offset: { top: this.offsetTop, left: this.offsetLeft },
        anchorAlign: { horizontal: 'right', vertical: 'bottom' },
        popupAlign: { horizontal: 'right', vertical: 'top' },
        popupClass: 'arrow-right'
      });
    }
  }

  //--------------------------------------------------------------------------------
  isEasyStudioDocument(object) {
    return this.easystudioService.isEasyStudioDocument(object);
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
    if(!this.easystudioService.isContextActive() || (customization !== null && !this.isCustomizationEnabled(customization)))     
      return;
    let custName = customization !== null ? customization.customizationName : undefined;
    this.easystudioService.runEasyStudio(object.target, custName);
    this.close();
  }
  
  /*//--------------------------------------------------------------------------------
  cloneAsEasyStudioDocumentIfNeeded(object) {
    // if (!(this.currentApplication !== undefined && this.currentApplication !== null && this.currentModule !== undefined && this.currentModule !== undefined)) {
    //   this.openContextMenu = true;
    //   let sub = this.httpMenuService.cloneAsEasyStudioDocument(object).subscribe((result) => {
    //     alert("clone lanciato");
    //     sub.unsubscribe();
    //   });
    // }
    // else {
      if(!this.easystudioService.isContextActive())     
        return;
      this.easystudioService.cloneDocument(object);
      this.close();
  }*/

  //--------------------------------------------------------------------------------
  private contains(target: any): boolean {
    return this.anchor.nativeElement.contains(target);
  }

  //--------------------------------------------------------------------------------
  public getToolTip(customization: EsCustomizItem) {
    let message = "";
    message += this.localizationService.localizedElements.CustomizationNotActive + ":\n";
    message += customization.applicationOwner + " / " + customization.moduleOwner;
    return message;
  }


}
