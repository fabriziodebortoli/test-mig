import { TbHotlinkComboComponent } from './tb-hot-link-combo.component';
import { untilDestroy } from './../../commons/untilDestroy';

export class TbHotlinkComboHyperLinkHandler {
    static Attach(hlb: TbHotlinkComboComponent): TbHotlinkComboHyperLinkHandler {
        return new TbHotlinkComboHyperLinkHandler(hlb);
    }

    private getHotLinkElement: () => HTMLElement;
    private constructor (hlc: any) {
        this.getHotLinkElement = () => {
            let searchBar = (hlc.vcr.element.nativeElement.parentNode.getElementsByClassName('k-searchbar') as HTMLCollection).item(0) as HTMLElement;
            if(searchBar) return (searchBar.getElementsByClassName('k-input') as HTMLCollection).item(0) as HTMLElement;
            return undefined; 
        }
        hlc.hyperLinkService.start(hlc,
            () => this.getHotLinkElement(),
            null, 
            { name: hlc.hotLinkInfo.name,
              cmpId: hlc.documentService.mainCmpId, 
              controlId: hlc.modelComponent.cmpId,
              enableAddOnFly: hlc.hotLinkInfo.enableAddOnFly, 
              mustExistData: hlc.hotLinkInfo.mustExistData,
              model: hlc.modelComponent.model 
            },
            hlc.slice$, hlc.afterNoAddOnFly, hlc.afterAddOnFly, hlc.onControlFocusLost);

        hlc.queryTrigger.pipe(untilDestroy(hlc)).subscribe(p => { 
            hlc.hyperLinkService.workingValue = p.model;
            hlc.hyperLinkService.workingType = hlc.state.selectionType;
        });
    }
}