import { ReportingStudioService } from './../../reporting-studio.service';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-askdialog',
  templateUrl: './askdialog.component.html',
  styleUrls: ['./askdialog.component.scss']
})
export class AskdialogComponent implements OnInit {

  @Input() ask: string;

  constructor(private rsSerivce: ReportingStudioService) {

    this.ask = 'Test';
  }

  ngOnInit() {
  }

}
