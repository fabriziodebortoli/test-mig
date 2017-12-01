import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-action-panel',
  templateUrl: './action-panel.component.html',
  styleUrls: ['./action-panel.component.css']
})
export class ActionPanelComponent implements OnInit {

  @Input() iconSignature: string;
  @Input() message: string;
  @Input() actionText: string;
  @Input() actionLink: string;
  @Input() showButton: boolean;
  @Output() actionFired: EventEmitter<boolean>;

  constructor() { 
    this.iconSignature = '';
    this.message = '';
    this.actionText = '';
    this.actionLink = '';
    this.showButton = true;
    this.actionFired = new EventEmitter<boolean>();
  }

  ngOnInit() {
  }

  fireActionEvent() {
    this.actionFired.emit();
  }

}
