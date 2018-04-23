import  * as _ from 'lodash';

export default class Esr {
    static checkEsrDigit(s: string): {result: boolean, error: string} {

        let	strNewValue = s;
        const valid = {result: true, error: ''};

        if (strNewValue === '')
            return valid;

        let length = strNewValue.length;
        let finalCheckDigit: string;

        // Se il codice è 27, 16 o < 15 faccio il controllo Modulo10.
        if (length === 27 || length === 16 || length < 15) {
            strNewValue = _.padStart(strNewValue, 27, '0');
            // non prendo l'ultimo, perche' e' il carattere di controllo
            finalCheckDigit = this.calculatePVRCheck(strNewValue.substring(0, 26));
            let expectedValue = strNewValue.substring(strNewValue.length - 1, strNewValue.length);
            if (finalCheckDigit !== expectedValue) {
                return { result: false,
                    error: 'Incorrect ESR Check Digit, value expected ' + finalCheckDigit + ' value found ' + expectedValue +  '.'};
            }
        }
        // Se il codice è esattamente di 15 non faccio nulla, dovrei fare il controllo Modulo10 ma non ho tutti i dati per farlo
        else if (length === 15) { }
        // Se la lunghezza è maggiore di 16 e non e' 27 allora il codice è sbagliato e do' un errore 
        else {
            return { result: false,
                error: 'Wrong length for ESR Reference Number'};
        }

        return valid;
    }

    static calculatePVRCheck(number: string) : string
    {
        // CheckDigit Modulo10
        // Creo un array di oggetti CString che a loro volta sono array di Char, quindo ho una matrice a 2 dimensioni...
        let aMatrix = [
            '0946827135 0',
            '9468271350 9',
            '4682713509 8',
            '6827135094 7',
            '8271350946 6',
            '2713509468 5',
            '7135094682 4',
            '1350946827 3',
            '3509468271 2',
            '5094682713 1'
        ];

        let checkCode = '';
        let riporto = 0, digit = 0;
        for (let i = 0; i < number.length; i++) {
            digit = Number(number[i]);
            riporto	= Number(aMatrix[riporto][digit]);
        }

        checkCode = aMatrix[riporto][11];
        return checkCode;
    }
}
