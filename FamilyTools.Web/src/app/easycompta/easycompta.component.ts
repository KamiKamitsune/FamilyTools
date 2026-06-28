import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-easycompta',
  imports: [RouterOutlet],
  templateUrl: './easycompta.component.html',
  styleUrl: './easycompta.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EasycomptaComponent {
  title = 'EasyCompta';
}
