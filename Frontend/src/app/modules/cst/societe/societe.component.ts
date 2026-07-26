import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-societe',
  standalone: true,
  imports: [
      RouterOutlet
  ],
  templateUrl: './societe.component.html',
  styleUrl: './societe.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SocieteComponent {

}
