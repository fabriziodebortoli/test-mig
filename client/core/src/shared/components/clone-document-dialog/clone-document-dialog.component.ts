import { MatSnackBar } from '@angular/material';
import { EasystudioService } from './../../../core/services/easystudio.service';
import { OldLocalizationService } from './../../../core/services/oldlocalization.service';
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
        public localizationService: OldLocalizationService,
        public easystudioService: EasystudioService,
        public snackBar: MatSnackBar) { }

    cancel() {
        this.openCloneDialog = false;
    }

    open() {
        if (!this.easystudioService.isContextActive())
            return;
        this.openCloneDialog = true;
    }

    okClone(object: any, docName: string, docTitle: string) {
        this.easystudioService.cloneDocument(object, docName, docTitle);
        this.cancel();
        this.snackBar.open(this.localizationService.localizedElements.NewDocumentCreatedwithSuccess, this.localizationService.localizedElements.Ok);
       
    }

}