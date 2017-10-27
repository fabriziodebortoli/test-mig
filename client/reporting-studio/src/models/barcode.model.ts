import { baserect } from './baserect.model';
import { BarcodeType } from './barcode-type.model';


export class barcode {

    type: string;
    includetext: boolean;
    rotate: string;
    constructor(jsonObj) {
        this.includetext = jsonObj.includetext;
        this.rotate = jsonObj.rotate ? "L" : "N";
        this.type = this.getBarcodeType(jsonObj.type);

    }

    private getBarcodeType(type: BarcodeType): string {
        switch (type) {
            case BarcodeType.CODABAR:
                return 'rationalizedCodabar';
            case BarcodeType.CODE128:
            case BarcodeType.CODE128A:
            case BarcodeType.CODE128B:
            case BarcodeType.CODE128C:
                return 'code128';
            case BarcodeType.CODE39:
                return 'code39';
            case BarcodeType.CODE93:
                return 'code93';
            case BarcodeType.DATAMATRIX:
                return 'datamatrix';
            case BarcodeType.EAN13:
                return 'ean13';
            case BarcodeType.EAN8:
                return 'ean8';
            case BarcodeType.EANJAN13:
                return 'ean13';
            case BarcodeType.EANJAN8:
                return 'ean8';
            case BarcodeType.EXT39:
                return 'code39ext';
            case BarcodeType.EXT93:
                return 'code93ext';
            case BarcodeType.HIBC:
                return 'hibccode128';
            case BarcodeType.INT25:
                return 'code2of5';
            case BarcodeType.MICROQR:
                return 'microqrcode';
            case BarcodeType.MSIPLESSEY:
                return 'msi';
            case BarcodeType.PDF417:
                return 'pdf417';
            case BarcodeType.QR:
                return 'qrcode';
            case BarcodeType.UCC128:
            case BarcodeType.EAN128:
                return 'gs1-128';
            case BarcodeType.UPCA:
                return 'upca';
            case BarcodeType.UPCE0:
            case BarcodeType.UPCE1:
            case BarcodeType.UPCE:
                return 'upce';
            case BarcodeType.ZIP:
                return 'postnet';

        }
    }
}