import { QueryList, AfterContentInit } from '@angular/core';
import { LayoutService } from './../../../services/layout.service';
import { TileGroupComponent } from './../tile-group/tile-group.component';
export declare class TileManagerComponent implements AfterContentInit {
    private layoutService;
    tiles: QueryList<TileGroupComponent>;
    getTiles(): TileGroupComponent[];
    private viewHeightSubscription;
    viewHeight: number;
    constructor(layoutService: LayoutService);
    ngOnInit(): void;
    ngOnDestroy(): void;
    ngAfterContentInit(): void;
    selectTile(tile: TileGroupComponent): void;
    changeTabByIndex(event: any): void;
}
