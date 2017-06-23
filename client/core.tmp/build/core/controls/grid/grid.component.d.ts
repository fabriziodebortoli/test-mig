import { OnInit, OnDestroy } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { DataService } from './../../services/data.service';
export declare class GridComponent implements OnInit, OnDestroy {
    private dataService;
    gridNamespace: string;
    gridSelectionType: string;
    gridParams: URLSearchParams;
    private dataSubscription;
    private gridColumns;
    private gridData;
    constructor(dataService: DataService);
    ngOnInit(): void;
    ngOnDestroy(): void;
}
