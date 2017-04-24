import { ReportingStudioService } from './../../reporting-studio.service';
import { Component, OnInit, Input } from '@angular/core';
import { TemplateItem } from './../../reporting-studio.model';

@Component({
  selector: 'rs-askdialog',
  templateUrl: './askdialog.component.html',
  styleUrls: ['./askdialog.component.scss']
})
export class AskdialogComponent implements OnInit {

  @Input() ask: string;

public objects:any[]=[];
public template:TemplateItem[]=[];

  constructor(private rsSerivce: ReportingStudioService) {
  }

  ngOnInit() {
  }

   RenderLayout() {
   }

}
