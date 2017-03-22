import { GridModule } from '@progress/kendo-angular-grid';
import { table } from './../../../reporting-studio/reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss']
})
export class ReportTableComponent implements OnInit {

  @Input() table: table;

  public data: any[] = [{ "col1": "", "col3": "", "col4": "Full Stops", "col5": "DD/MM/YY", "col6": "European", "col11": "", "col12": "", "col13": "", "col14": "", "col15": "Yes", "col16": "No" }/*, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Commas", "5": "DD/MM/YY", "6": "PM-AM", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Commas", "5": "DD/MM/YY", "6": "PM-AM", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "No", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "Full Stops", "5": "DD/MM/YY", "6": "European", "11": "", "12": "", "13": "", "14": "", "15": "Yes", "16": "No" }, { "1": "", "3": "", "4": "", "5": "", "6": "", "11": "", "12": "", "13": "", "14": "", "15": "", "16": "" }*/];

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectTableComponent', this.ro);
  }

}
