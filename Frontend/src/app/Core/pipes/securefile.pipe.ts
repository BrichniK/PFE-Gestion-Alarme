import { inject, Pipe, PipeTransform } from '@angular/core';
import { ApiService } from '../common/api.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { map, Observable } from 'rxjs';

@Pipe({
    name: 'securefile',
    standalone: true
})
export class SecurefilePipe implements PipeTransform {

    private _apiService = inject(ApiService);
    private _sanitizer = inject(DomSanitizer);

    transform(url: string): Observable<SafeUrl> {
        return this._apiService.GetImage(url)
            .pipe(
                map(val =>
                    this._sanitizer.bypassSecurityTrustUrl(
                        URL.createObjectURL(val))
                ));
    }

}
