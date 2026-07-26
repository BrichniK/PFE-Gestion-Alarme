import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'decimalFormat',
  standalone: true
})
export class DecimalFormatPipe implements PipeTransform {
  transform(value: number | string | null | undefined, decimals: number = 3): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }

    const raw = String(value).replace(',', '.').trim();
    const match = raw.match(/^(-?\d+)(?:\.(\d*))?$/);
    if (!match) {
      return '';
    }

    const integerPart = match[1];
    let decimalPart = match[2] || '';

    if (decimals < 0) {
      decimals = 0;
    }

    if (decimalPart.length > decimals) {
      decimalPart = decimalPart.slice(0, decimals);
    }
    while (decimalPart.length < decimals) {
      decimalPart += '0';
    }

    return decimals === 0 ? integerPart : `${integerPart}.${decimalPart}`;
  }
}

