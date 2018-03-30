import { Component, Input, ChangeDetectorRef, ViewChild, Output, EventEmitter } from '@angular/core';
import { EventDataService } from '../../../core/services/eventdata.service';
import { TbComponentService } from '../../../core/services/tbcomponent.service';
import { TbComponent } from '../tb.component';
import { ComponentMediator } from '../../../core/services/component-mediator.service';
import { ExplorerComponent, ExplorerItem, ExplorerOptions, ExplorerResult } from './explorer.component';
export { ExplorerComponent, ExplorerItem, ExplorerOptions, ExplorerResult } from './explorer.component';
import { Observable, Subject, Observer } from '../../../rxjs.imports';
import { async as asyncScheduler } from 'rxjs/scheduler/async';
import { get } from 'lodash';

/**
 * @example
 * template: <tb-explorer-dialog></tb-explorer-dialog>
 * ts:
 * @ViewChild(ExplorerDialogComponent) explorer: ExplorerDialogComponent;
 * this.explorer.open({ objType: ObjType.Document })
 *     .subscribe(selectedItems => console.log('item selected'));
 */
@Component({
    selector: 'tb-explorer-dialog',
    templateUrl: './explorer-dialog.component.html',
    styleUrls: ['./explorer-dialog.component.scss'],
})
export class ExplorerDialogComponent extends TbComponent {
    opened = false;
    @Input() options: ExplorerOptions;
    @Output() result = new EventEmitter<ExplorerResult>();
    @ViewChild(ExplorerComponent) explorer: ExplorerComponent;
    private close$ = new Subject<ExplorerItem>();

    constructor(tbComponentService: TbComponentService, changeDetectorRef: ChangeDetectorRef) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();
    }

    open(options: ExplorerOptions): Observable<ExplorerResult> {
        this.options = new ExplorerOptions(options);
        this.opened = true;
        setTimeout(() => this.explorer.focus());
        return Observable.create(o => {
            setTimeout(() => {
                this.close$
                    .merge(this.explorer.selectionChanged.asObservable())
                    .first()
                    .do(() => this.close())
                    .map(s => new ExplorerResult(s && [new ExplorerItem(s.name, s.namespace)]))
                    .subscribe(o);
            });
        });
    }

    close() { this.close$.next(); this.opened = false; }
}
