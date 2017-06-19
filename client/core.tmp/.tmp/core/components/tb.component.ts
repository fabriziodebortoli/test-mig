import { Input } from '@angular/core';

export abstract class TbComponent {

  @Input() public cmpId = '';

  constructor() { }

}
