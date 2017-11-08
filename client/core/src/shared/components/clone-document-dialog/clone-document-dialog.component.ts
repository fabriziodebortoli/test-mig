import { EasystudioService } from './../../../core/services/easystudio.service';
import { LocalizationService } from './../../../core/services/localization.service';
import { Component, ViewChild, ElementRef, OnInit, AfterViewInit, OnDestroy, Input } from '@angular/core';

@Component({
    selector: 'tb-clone-doc-dialog',
    templateUrl: './clone-document-dialog.component.html',
    styleUrls: ['./clone-document-dialog.component.scss']
})


export class CloneDocumentDialogComponent {

    @Input() object: any;
    public docName: string;
    public docTitle: string;
    public openCloneDialog = false;
  
    constructor(
        public localizationService: LocalizationService,
        public easystudioService: EasystudioService) { }

     cancel(){
        this.openCloneDialog = false;
     }
     open(){
         this.openCloneDialog = true;
     }
     okClone(object: any, docName: string, docTitle:string){
         this.easystudioService.cloneDocument(object.target);
     }

}