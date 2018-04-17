import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { ExplorerService } from './../../../core/services/explorer.service';
import { InfoService } from './../../../core/services/info.service';

import {
  Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, ElementRef,
  OnChanges, AfterContentInit, OnInit, Output, HostListener, EventEmitter, ChangeDetectorRef, SimpleChanges
} from '@angular/core';
import { Store } from './../../../core/services/store.service';

import { EventDataService } from './../../../core/services/eventdata.service';
import { ObjType } from '../../../core/services/explorer.service';

import { ControlComponent } from '../control.component';
import { Subscription } from '../../../rxjs.imports';
import { ExplorerDialogComponent } from '../../components/explorer/explorer-dialog.component';
import { FormMode, createSelector, createSelectorByPaths } from './../../../shared/shared.module';
import { Selector } from './../../../shared/models/store.models';
import { EventHandler } from './event-handler';

import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryComponent } from 'ngx-gallery';

@Component({
  selector: 'tb-namespace',
  templateUrl: './namespace.component.html',
  styleUrls: ['./namespace.component.scss']
})
export class NamespaceComponent extends ControlComponent implements OnChanges, OnInit {
  @Input('readonly') readonly: boolean = false;
  @Input('chars') chars: number = 0;
  @Input('rows') rows: number = 0;
  @Input('textLimit') textlimit: number = 0;
  @Input('maxLength') maxLength: number = 524288;

  @Input('namespaceType') namespaceType: string = "Report";
  @Input('defaultNS') defaultNs: string = "";
  @Input() slice: any;
  @Input() selector: Selector<any, any>;

  errorMessage: any;
  objectType: ObjType = ObjType.Report;
  user: string;
  company: string;
  culture: string;
  controlClass: string;

  @ViewChild(ExplorerDialogComponent) explorer: ExplorerDialogComponent;
  @ViewChild('textArea') textArea: ElementRef;
  @ViewChild('gal') gal: NgxGalleryComponent;

  galleryImages: NgxGalleryImage[] = [];
  galleryOptions: NgxGalleryOptions[] = [{
    closeIcon: 'm4-icon m4-tb-closewindows-2',
    previewCloseOnEsc: true,
    previewCloseOnClick: true
    image: false, 
    thumbnails: false, 
    width: "0px", 
    height: "0px"
  }];

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    private infoService: InfoService,
    private explorerService: ExplorerService,
    //private menuService: MenuService,
    private store: Store
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    this.store.select(_ => this.model && this.model.length).
      subscribe(l => this.onlenghtChanged(l));

    this.user = localStorage.getItem('_user');
    this.company = localStorage.getItem('_company');
    this.culture = localStorage.getItem('ui_culture');

    if (this.namespaceType != undefined) {
      this.objectType = ObjType[this.namespaceType];
      if (this.objectType === undefined)
        console.log('Wrong objectType in ' + this.cmpId);
    }
  }

  ngAfterContentInit() { EventHandler.Attach(this); }

  search($event) {
    this.explorer.open({ objType: this.objectType, upload: this.objectType == ObjType.Image || this.objectType == ObjType.Report })
      .subscribe(this.onsearch);
  }

  onsearch = async obj => {
    if (obj.items.length > 0) {
      this.model.value = ObjType[this.objectType] + '.' + obj.items[0].namespace;
      this.showDescription(obj.items[0].name);
      this.changeDetectorRef.detectChanges();
    }
  }

  onlenghtChanged(len: any) {
    if (len !== undefined)
      this.setlength(len);
  }

  async onBlur($event) {
    if (!await this.validate()) {
      this.errorMessage = this._TB('Incorrect namespace.');
      this.changeDetectorRef.detectChanges();
      return;
    }

    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

  async showDescription(name: string) {
    if (this.store && this.selector && name != undefined) {
      let slice = await this.store.select(this.selector).take(1).toPromise();
      if (slice && slice.description)
        slice.description.value = name;
      this.changeDetectorRef.markForCheck();
    }
  }

  async validate(): Promise<boolean> {
    this.errorMessage = '';
    // Vuoto va bene !!!
    if (this.value === '') {
      this.showDescription("");
      return Promise.resolve(true);
    }

    // Se non ci sono punti, non sono un namespace
    const elem: string = this.value.split('.');
    if (elem.length < 3) {
      this.showDescription("");
      return Promise.resolve(false);
    }

    // Se il primo token non è nei predefiniti non sono un namespace valido
    if (ObjType[elem[0]] === undefined || ObjType[elem[0]] != this.objectType) {
      this.showDescription("");
      return Promise.resolve(false);
    }

    // Guardo se trovo il file selezionato tramite le API
    let ok = await this.explorerService.ExistsObject(this.value, this.user, this.company, this.culture);

    // Se ho il selector allora decodifico l'oggetto
    if (this.selector) {
      let desc = ok ? await this.explorerService.GetDescription(this.value, this.user, this.company, this.culture) : '';
      this.showDescription(desc);
    }

    return Promise.resolve(ok);
  }

  // Metodo con OnChanges
  ngOnChanges(changes: SimpleChanges) {
    if (!changes.model || !this.model)
      return;

    this.setlength(this.model.length)
  }

  setlength(len: number) {
    if (this.model && this.model.length > 0)
      this.maxLength = this.model.length;

    if (this.textlimit > 0 && (this.maxLength == 0 || this.textlimit < this.maxLength)) {
      this.maxLength = this.textlimit;
    }
  }

  isImage(){
    return this.objectType == ObjType.Image;
  }

  getImages(namespace: string) {
    if (!this.isImage() || !namespace) return [];

    return [
      {
        small: this.getImageUrl(namespace),
        big: this.getImageUrl(namespace),
        url: this.getImageUrl(namespace)
      }
    ];
  }

  onClick() {
    switch (this.objectType) {
      case ObjType.Image:
        this.gal.openPreview(0);
        break;
      default:
        //this.menuService.runObject({ objectType: ObjType[this.objectType], target: this.model.value });
        break;
    }
  }

  getImageUrl(namespace: string) {
    return this.infoService.getDocumentBaseUrl() + 'getImage/?src=' + namespace;
  }

}
