import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMPANYService } from './IDD_COMPANY.service';

@Component({
    selector: 'tb-IDD_COMPANY',
    templateUrl: './IDD_COMPANY.component.html',
    providers: [IDD_COMPANYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COMPANYComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_COMPANY_LEGALNATURE_COMBO_itemSource: any;
public IDC_COMPANY_COMMISS_itemSource: any;
public IDC_COMPANY_STATUS_itemSource: any;
public IDC_COMPANY_ADDRESSES_COMMISS_itemSource: any;
public IDC_COMPANY_ADDRESSES_STATUS_itemSource: any;
public IDC_COMPANY_PERSON_itemSource: any;
public IDC_COMPANY_PROVINCEN_itemSource: any;
public IDC_COMPANY_PEOPLE_COUNTY_itemSource: any;
public IDC_COMPANY_TS_OWNER_TYPE_itemSource: any;
public IDC_COMPANY_CHAMBOFCOMMCOUNTY_itemSource: any;
public IDC_COMPANY_PROFESSIONALREGISTERCOUNTY_itemSource: any;
public IDC_COMPANY_COMMISS_INTENDANCY_itemSource: any;
public IDC_COMPANY_SOTT_COD_LOAD_itemSource: any;
public IDC_TAXP_DECLARATIONTYPE_itemSource: any;
public IDC_TAXP_COUNTY_BIRTH_itemSource: any;
public IDC_TAXP_COUNTY_DF_itemSource: any;
public IDC_TAXP_COUNTY_SF_itemSource: any;

    constructor(document: IDD_COMPANYService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_COMPANY_LEGALNATURE_COMBO_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.LegalNature"
}; 
this.IDC_COMPANY_COMMISS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_COMPANY_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_COMPANY_ADDRESSES_COMMISS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_COMPANY_ADDRESSES_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_COMPANY_PERSON_itemSource = {
  "name": "VarEnumCombo",
  "namespace": "ERP.Company.Documents.VarEnumCombo"
}; 
this.IDC_COMPANY_PROVINCEN_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_COMPANY_PEOPLE_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_COMPANY_TS_OWNER_TYPE_itemSource = {
  "name": "TSOwnerTypeCombo",
  "namespace": "ERP.TESANConnector.AddOnsCompany.TSOwnerTypeCombo"
}; 
this.IDC_COMPANY_CHAMBOFCOMMCOUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_COMPANY_PROFESSIONALREGISTERCOUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_COMPANY_COMMISS_INTENDANCY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_COMPANY_SOTT_COD_LOAD_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Payees.PositionCode"
}; 
this.IDC_TAXP_DECLARATIONTYPE_itemSource = {
  "name": "TelematicDeclTypeITEnumCombo",
  "namespace": "ERP.Company.Documents.TelematicDeclTypeITEnumCombo"
}; 
this.IDC_TAXP_COUNTY_BIRTH_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_TAXP_COUNTY_DF_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_TAXP_COUNTY_SF_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 

        		this.bo.appendToModelStructure({'Company':['LegalNature','LegalNature','FedStateReg','MunicipalityReg','SUFRAMA','GenRegNo','GenRegEntity','Address','Address2','City','ZIPCode','County','Country','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','Country','Telephone1','Telephone2','Telex','Fax','EMail','CertifiedEMail','Internet','ActivityCode','Artisan','SeasonalBusiness','BusinessInMoreLocations','PartialBookKeeping','TSOwnerType','TSOwnerTypeDescri','ChambOfCommRegistrNo','ChambOfCommArea','ChambOfCommCounty','CompanyRegistrNo','CommRegisetrRegistrNo','CourtRegistrationId','ImportExportLicence','CooperativeRegistrNo','ArtisanRegistrNo','ArtisanArea','ProfessionalRegisterName','ProfessionalRegisterCounty','ProfessionalRegisterNo','ProfessionalRegisterDate','ProfessionalCashType','CollectorsOffice','TaxCANo','Court','TerritoryType','TaxPayerCode','AuthorizationCode','RevenueOfficeCounty','LicenceNumber','LicenceDate','InLiquidation','SoleShareholder','SubmittedToCoordination','CoordinatorName','BelongToGroup','GroupName','GroupCountry','EORICode','LicenceDate','SIACode','CreditorIdentifier','CBICode','UseTariffValueICMS','IntrastatUserCode','CustomsSectionCode','CustomsTaxIdNumber','IntrastatProgNo','SocialSecurityCode','SocialSecurityId','SocialInsuranceBranch','SocialInsuranceId','RegionCode','INPDAIENPALSEntity','INPDAIENPALSBranch','INPDAIENPALSCode','ENASARCORegistrationId','SubscriberFiscalCode','LoadingPlace','LoaderType','LoaderCode','LoaderBranch','FantasyName','GoodsActivity','ServicesActivity'],'global':['Branches','People','FiscalYears','Years','Tax','SubscriberChargeCode_XML','TaxDeclaration','LoadingPlaceDescri','LoaderDescri','CompanyFedStateReg','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Branches':['Branch','TitleCode','BranchType','CompanyName','FantasyName','ISOCountryCode','TaxIdNumber','FederalState','ZIPCode','Address','StreetNo','Address2','District','City','City','ZIPCode','County','Country','Telephone1','Telephone2','Telex','Fax','Internet','EMail'],'People':['Person','TitleCode','LastName','Name','DomicileFC','Gender','DateOfBirth','CountyOfBirth','CityOfBirth','DomicileZip','DomicileCounty','DomicileAddress','DomicileCity','ISOCountryCodeOfBirth','CityOfBirth','CountyOfBirth','DomicileAddress','DomicileCity','DomicileZip','DomicileCounty','Telephone1','Telephone2','Fax','EMail'],'HKLActivityCodes':['Description'],'FiscalYears':['TEnhCompanyFiscalYears_P1','OpeningDate','ClosingDate','TEnhCompanyFiscalYears_P2','DepreciationDate','FinalOpeningDate','TemporaryClosingDate','TemporaryClosingCustSupp','ProfitLossClosingDate','FinalClosingDate','BlockAccountingPosting','BlockCostAccPosting','BlockFixedAssetsPosting','BlockInventoryPosting','BlockInventoryPostingDate','Notes','ExceptionalEvents','UseItemDefaultValuation','InitialInventoryBalances','FromMagoNet'],'Years':['BalanceYear','MixedRegime','TaxRegulations','SalesTaxPerc','SalesTaxPerc2','QuarterlyTaxSettlement','QuarterlyTaxOption','UseTaxDistribution','TAXExigibilityOnCollection','TAXExigibilityCashRegime','TAXExigibilityCashRegimeFrom','TAXExigibilityCashRegimeTo','FarmerTax','TemporaryProRataPerc','FinalProRataPercc','FinalProRataDiff','Plafond','PreviousYearTurnover','PreviousYearExport','IntrastatPurchasesChangeDate','IntrastatPurchases','IntrastatSalesChangeDate','IntrastatSales','PurchasesStatisticalValue','SalesStatisticalValue','Factor','QuarterlyBlackList','TaxAdvance','TaxAdvanceDescri'],'Tax':['FromDate','ToDate','TaxType'],'TaxDeclaration':['TelematicDeclarationType','ExtactData','FiscalCode','TaxIdNumber','CAFRegistrationNo','NaturalPerson','LastName','Name','Gender','DateOfBirth','ISOCountryCodeOfBirth','CityOfBirth','CountyOfBirth','FiscalDomicileCity','FiscalDomicileCounty','FiscalDomicileAddress','FiscalDomicileZip','RegisteredOfficeCity','RegisteredOfficeCounty','RegisteredOfficeAddress','RegisteredOfficeZip'],'HKLGoodsActivityCode':['Description'],'HKLServicesActivityCode':['Description'],'CompanyFedStateReg':['FederalState','FedStateReg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMPANYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMPANYComponent, resolver);
    }
} 