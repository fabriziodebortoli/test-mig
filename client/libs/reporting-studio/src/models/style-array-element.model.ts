import { cell } from './cell.model';

export class styleArrayElement {
    rowNumber: number;
    public style: cell[] = []
    constructor(row: number) {
        this.rowNumber = row;
    }
}