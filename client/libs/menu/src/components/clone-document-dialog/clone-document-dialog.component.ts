import { TbComponent, TbComponentService } from '@taskbuilder/core';
import { Component, ViewChild, ElementRef, OnInit, AfterViewInit, OnDestroy, Input, ChangeDetectorRef } from '@angular/core';

import { EasystudioService } from './../../services/easystudio.service';

@Component({
    selector: 'tb-clone-doc-dialog',
    templateUrl: './clone-document-dialog.component.html',
    styleUrls: ['./clone-document-dialog.component.scss']
})
export class CloneDocumentDialogComponent extends TbComponent {

    @Input() object: any;
    public docName: string;
    public docTitle: string;
    public openCloneDialog = false;

    constructor(
        public easystudioService: EasystudioService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();
    }

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
        // this.snackBar.open(this._TB('New Document Created with Success'), this._TB('Ok'));       
        alert("TO DO - utilizzage diagnostic - " + this._TB('New Document Created with Success'));
    }

}