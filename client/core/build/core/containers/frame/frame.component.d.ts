import { OnInit, OnDestroy } from '@angular/core';
import { LayoutService } from './../../services/layout.service';
export declare class FrameComponent implements OnInit, OnDestroy {
    private layoutService;
    private viewHeightSubscription;
    viewHeight: Number;
    constructor(layoutService: LayoutService);
    ngOnInit(): void;
    ngOnDestroy(): void;
}
