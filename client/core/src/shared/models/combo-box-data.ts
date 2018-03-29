export class ComboBoxData { 
    id: string; 
    displayString: string;
    constructor(o?: Partial<ComboBoxData>) { if(o) Object.keys(o).map(k => this[k] = o[k]); }
    toUpperCase() {
      this.id = this.id.toUpperCase();
      this.displayString = this.displayString.toUpperCase();
    }
  }