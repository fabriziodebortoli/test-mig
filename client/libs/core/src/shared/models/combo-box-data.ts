export class ComboBoxData { 
    id: string; 
    displayString: string;
    constructor(o?: Partial<ComboBoxData>) { if(o) Object.keys(o).forEach(k => this[k] = o[k]); }
    toUpperCase(): ComboBoxData {
      this.id = this.id.toUpperCase();
      this.displayString = this.displayString.toUpperCase();
      return this;
    }
  }