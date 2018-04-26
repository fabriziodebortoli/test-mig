import { InfoService } from './../../../../../core/services/info.service';
import { Store } from './../../../../../core/services/store.service';
import { TbComponent } from '../../../../components/tb.component';
import { EventDataService } from './../../../../../core/services/eventdata.service';
import { TbComponentService } from './../../../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../../../core/services/layout.service';
import { ControlComponent } from './../../../../controls/control.component';
import { Component, Input, ChangeDetectorRef, OnDestroy, Injector, ViewContainerRef, QueryList, ContentChild, TemplateRef,
         ElementRef, AfterContentInit , AfterContentChecked} from '@angular/core';



@Component({
  selector: 'tb-status-tile',  
  templateUrl: './status-tile.component.html',
  styleUrls: ['./status-tile.component.scss']
})

export class StatusTileComponent extends ControlComponent implements OnDestroy {

  active: boolean = true;

  @Input() title: string;
  @Input() clickable: boolean;
  @Input() visible: boolean;
  @Input() backgroundColor: any;
  @Input() descr: any;
  @Input() up: any;
  @Input() image: any;
  imgUrl: string;
  _backGroundHexColors : string;
  DEFAULT_TILE_COLOR;

  constructor(layoutService: LayoutService, 
    changeDetectorRef: ChangeDetectorRef,
    tbComponentService: TbComponentService,
    public eventData: EventDataService,
    public store: Store,
    elRef: ElementRef,
    public infoService: InfoService) {
    super(layoutService,tbComponentService,changeDetectorRef);
    this.DEFAULT_TILE_COLOR = "RGB(255, 255, 255)";
    this.imgUrl = this.infoService.getDocumentBaseUrl() + 'getImage/?src=';
  }

  ngOnInit() {  
    this.store.select(_ => this.backgroundColor && this.backgroundColor.value).subscribe(c => this._backGroundHexColors = c ? 
        this.ExtractRGBValues(c) : this.DEFAULT_TILE_COLOR);
  }


   inputStyle(){

    return {      
        'border-radius': '5px',
        'border-color' : 'Black',
        'border-width' : 'thick',
        'margin-top' : '2px',
        'margin-right' : '8px',
        'margin-bottom' : '2px',
        'margin-left' : '8px',  
        'min-width' : '80px' ,  
        'min-height' : '50px' ,    
        'background': this._backGroundHexColors
      };
   }
ExtractRGBValues(RGBvalues){  
    try{
        var digits = /(.*?)RGB\((\d+), (\d+), (\d+)\)/.exec(RGBvalues);            
        return "#" + this.RGBComponentToHex(digits[2]) + this.RGBComponentToHex(digits[3]) + this.RGBComponentToHex(digits[4]);        
    }
    catch(e){
        console.log("Error while converting from RGB to Hex, default background color " + this.DEFAULT_TILE_COLOR +  " will be used. Error: " + e);
        return this.DEFAULT_TILE_COLOR;
    }
}
RGBComponentToHex(c) {   
    let number = new Number(c);   
    var hex = number.toString(16);
    return hex.length == 1 ? "0" + hex : hex;
}

}

