import { OperationResult } from './../../services/operationResult';
import { Component, OnInit, Input } from '@angular/core';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { ModelService } from '../../services/model.service';
import { ImportDataParameters, ImportDataBodyContent } from './helpers/database-helpers';
import { ImportExportConsts } from '../components.helper';
import { DataChannelService } from 'app/services/data-channel.service';

@Component({
  selector: 'app-import-data',
  templateUrl: './import-data.component.html',
  styleUrls: ['./import-data.component.css']
})

export class ImportDataComponent implements OnInit {

  @Input() model: SubscriptionDatabase;
  @Input() defaultInfo: Object; 
  @Input() sampleInfo: Object;

  isImporting: boolean = false;

  importDefaultData: boolean = true;
  importSampleData: boolean = false;

  // options
  importParams: ImportDataParameters;

  // lista country code
  countries: Array<{ name: string, value: string }> = [];
  selectedCountry: { name: string, value: string };
  //

  // lista configurazioni disponibili per i dati di default/esempio
  configurations: Array<{ name: string, value: string }> = [];
  selectedConfiguration: { name: string, value: string };
  //

  //-----------------------------------------------------------------------------	
  constructor(
    private modelService: ModelService,
    private dataChannelService: DataChannelService) {
      
     this.importParams = new ImportDataParameters();
  }

  //-----------------------------------------------------------------------------	
  ngOnInit() {

    this.countries.push(
      { name: 'IT', value: 'IT' },   // l'iso stato dovrebbe essere un'informazione a livello di subscription
      { name: ImportExportConsts.INTL, value: ImportExportConsts.INTL }// il set INTL viene aggiunto d'ufficio
    );

    // to initialize the default data configurations in dropdown
    this.dataChannelService.dataChannel.subscribe(
      (res) => {
        this.selectedCountry = { name: 'IT', value: 'IT' };
        this.onSelectedCountryChange(this.selectedCountry);
          },
      (err) => { }
    );
  }

  //--------------------------------------------------------------------------------------------------------
  onImportDefaultDataChanged(setImportDefaultData: boolean) {
    this.importDefaultData = setImportDefaultData;
    this.importSampleData = !this.importDefaultData;

    this.onSelectedCountryChange(this.selectedCountry);
  }

  //--------------------------------------------------------------------------------------------------------
  onImportSampleDataChanged(setImportSampleData: boolean) {
    this.importSampleData = setImportSampleData;
    this.importDefaultData = !this.importSampleData;

    this.onSelectedCountryChange(this.selectedCountry);
  }

  //-----------------------------------------------------------------------------	
  onSelectedCountryChange(countryCodeValue) {

    this.configurations = [];

    let values: Array<string> = [];

    if (this.importDefaultData) {
      for (let key in this.defaultInfo) {
        if (key === this.selectedCountry.name)
          values = this.defaultInfo[key];
      }
    }
    else
      if (this.importSampleData) {
        for (let key in this.sampleInfo) {
          if (key === this.selectedCountry.name)
            values = this.sampleInfo[key];
        }
      }

    values.forEach((item, index) => {
      this.configurations.push({ name: item, value: item })
    });

    // mi posiziono sulla prima configurazione
    this.selectedConfiguration = this.configurations[0];
  }

  //-----------------------------------------------------------------------------	
  onDeleteTableContentsChanged(value) {
    this.importParams.DeleteTableContext = value;
  }

  //-----------------------------------------------------------------------------	
  onSkipRowChanged(value) {
    this.importParams.OverwriteRecord = !value;
  }

  //-----------------------------------------------------------------------------	
  onUpdateRowChanged(value) {
    this.importParams.OverwriteRecord = value;
  }

  //-----------------------------------------------------------------------------	
  importData() {

    if (this.isImporting)
      return;

    if (this.selectedConfiguration == undefined || this.selectedCountry == undefined ||
      this.selectedConfiguration.value == undefined || this.selectedConfiguration.value === '' ||
      this.selectedCountry.value == undefined || this.selectedCountry.value === '') {
      alert('Country or configuration missing!');
      return;
    }

    let importBodyContent: ImportDataBodyContent = new ImportDataBodyContent();
    importBodyContent.Database = this.model;
    importBodyContent.ImportParameters = this.importParams;

    this.isImporting = true;

    let importData = this.modelService.importData(this.model.SubscriptionKey,
      this.importDefaultData,
      this.selectedConfiguration.value,
      this.selectedCountry.value,
      importBodyContent).
      subscribe(importDataResult => {

        this.isImporting = false;

        alert(importDataResult.Message);

        importData.unsubscribe();
      },
      importDataError => {
        alert(importDataError);
        this.isImporting = false;
        importData.unsubscribe();
      })
  }
}
