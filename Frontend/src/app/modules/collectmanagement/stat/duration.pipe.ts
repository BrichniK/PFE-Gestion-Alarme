import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'duration',
    standalone: true,
})
export class DurationPipe implements PipeTransform {
    transform(totalSeconds: number | null | undefined): string {
        if (totalSeconds == null) return '-';
        const abs = Math.abs(totalSeconds);
        const h = Math.floor(abs / 3600);
        const m = Math.floor((abs % 3600) / 60);
        const s = Math.floor(abs % 60);
        const sign = totalSeconds < 0 ? '-' : '';
        return `${sign}${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
    }
}
