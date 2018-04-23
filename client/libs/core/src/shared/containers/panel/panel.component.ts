import { Subscription } from 'rxjs';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { BOService } from './../../../core/services/bo.service';
import { Component, OnInit, Input, Output, ViewEncapsulation, EventEmitter, OnDestroy, ChangeDetectorRef, HostBinding } from '@angular/core';
import { TbComponent } from '../../components/tb.component';


@Component({
    selector: 'tb-panel',
    templateUrl: './panel.component.html',
    styleUrls: ['./panel.component.scss']
})
export class PanelComponent extends TbComponent implements OnInit, OnDestroy {

    _title: any;
    _collapsedTitle: any;

    subscriptions: Subscription[] = new Array<Subscription>();

    @HostBinding('class.collapsed') @Input() isCollapsed: boolean = false;
    @Input() isCollapsible: boolean = false;
    realTitle = ""
    @Output() toggle = new EventEmitter<boolean>();

    constructor(
        private boService: BOService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(tbComponentService, changeDetectorRef);
    }
    ngOnInit() {
        this.boService.activateContainer(this.cmpId, true, false);
    }
    ngOnDestroy() {

        this.boService.activateContainer(this.cmpId, false, false);
        this.subscriptions.forEach(element => {
            element.unsubscribe();
        });
    }

    @Input() public set title(val: any) {
        this._title = val;
        if (this._title) {
            if (this._title.valueChanged) {
                this.subscriptions.push(this._title.valueChanged.subscribe(sender => this.calculateRealTitle()));
            }
            if (this._title.modelChanged) {
                this.subscriptions.push(this._title.modelChanged.subscribe(sender => this.calculateRealTitle()));
            }
        }
        this.calculateRealTitle();
    }
    public get title(): any {
        return this._title;
    }
    @Input() public set collapsedTitle(val: string) {
        this._collapsedTitle = val;
        if (this._collapsedTitle) {
            if (this._collapsedTitle.valueChanged) {
                this.subscriptions.push(this._collapsedTitle.valueChanged.subscribe(sender => this.calculateRealTitle()));
            } if (this._collapsedTitle.modelChanged) {
                this.subscriptions.push(this._collapsedTitle.modelChanged.subscribe(sender => this.calculateRealTitle()));
            }
        }
        this.calculateRealTitle();
    }
    public get collapsedTitle(): string {
        return this._collapsedTitle;
    }

    toggleCollapse(): void {
        if (!this.isCollapsible)
            return;

        this.isCollapsed = !this.isCollapsed;
        this.calculateRealTitle();

        this.toggle.emit(this.isCollapsed);
    }

    collapse(): void {
        if (!this.isCollapsible)
            return;

        this.isCollapsed = true;
        this.calculateRealTitle();
    }

    expand(): void {
        if (!this.isCollapsible)
            return;

        this.isCollapsed = false;
        this.calculateRealTitle();
    }

    calculateRealTitle() {
        if (this.isCollapsed && this._collapsedTitle) {
            if (this._collapsedTitle.hasOwnProperty('value')) {
                this.realTitle = this._collapsedTitle['value'];
            }
            else {
                this.realTitle = this._collapsedTitle;
            }

        } else if (this._title) {
            {
                if (this._title.hasOwnProperty('value')) {
                    this.realTitle = this._title['value'];
                }
                else {
                    this.realTitle = this._title;
                }
            }
        }

    }
}
