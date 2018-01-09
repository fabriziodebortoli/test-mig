import { TbComponent } from './../../../shared/components/tb.component';
import { MatSnackBar } from '@angular/material';
import { EasystudioService } from './../../../core/services/easystudio.service';
import { Component, ViewChild, ElementRef, OnInit, AfterViewInit, OnDestroy, Input, ChangeDetectorRef } from '@angular/core';
import { TbComponentService } from './../../../core/services/tbcomponent.service';

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
        public snackBar: MatSnackBar,
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
        this.snackBar.open(this._TB('New Document Created with Success'), this._TB('Ok'));
       
    }

}