import { Component, Input, ChangeDetectorRef, OnChanges, ViewChild, OnInit } from '@angular/core';
import { ControlComponent } from '@taskbuilder/core';
import { EventDataService } from '@taskbuilder/core';
import { TbComponentService } from '@taskbuilder/core';
import { LayoutService } from '@taskbuilder/core';
import { Store } from '@taskbuilder/core';

@Component({
  selector: 'erp-no-spaces',
  templateUrl: './no-spaces.component.html',
  styleUrls: ['./no-spaces.component.scss']
})
export class NoSpacesEditComponent extends ControlComponent implements OnChanges {

  readonly INITIAL_SEGMENT_LENGTH = 8;

  @Input('readonly') readonly: boolean = false;
  @Input() slice: any;
  @Input() selector: any;
  @Input() public hotLink: { namespace: string, name: string };

  @ViewChild('textbox') textbox: any;

  errorMessage = '';
  subscribedToSelector = false;
  maxLength = -1;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    private store: Store
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    this.textbox.input.nativeElement.maxLength = this.INITIAL_SEGMENT_LENGTH;
  }

  ngOnChanges(changes) {
    this.subscribeToSelector();
  }

  subscribeToSelector() {
    // maxLength is an optional parameter, i may not have to use it.
    // It is also the only parameter, so i have no selector without it
    if (!this.subscribedToSelector && this.store && this.selector) {
      this.store
        .select(this.selector)
        .select('maxLength')
        .subscribe(
        (v) => {
          if (v) {
            this.maxLength = v;
            this.textbox.input.nativeElement.maxLength = v;
          }
        }
        );

      this.subscribedToSelector = true;
    }
  }

  onKeyDown($event) {
    if ($event.keyCode === 32) {
      $event.preventDefault();
    }
  }

  removeSpaces() {
    if (this.model && this.model.value)
      this.model.value = this.model.value.replace(/\s+/g, '');

    if (this.maxLength > 0 && this.model.value.length !== this.maxLength)
      this.errorMessage = this._TB('Value length must be of ') + this.maxLength + this._TB(' chars');
    // this.errorMessage = this._TB('Value must be {0} chars', this.maxLength); Versione definitiva, dopo le modifiche di MArco
    else
      this.errorMessage = '';

    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }
}
