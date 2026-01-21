import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../../../service/accountService/account.service';
import { AccountEnter } from '../../../models/account-enter';
import { AccountTag } from '../../../models/account-tag';

@Component({
  selector: 'app-edit-account-enter',
  imports: [],
  templateUrl: './edit-account-enter.component.html',
  styleUrl: './edit-account-enter.component.css',
})
export class EditAccountEnterComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private service = inject(AccountService)
  // private accountEnterService = inject(AccountEnterService);

  accountEnter = signal<AccountEnter | undefined>(undefined);
  tags = signal<AccountTag[]>([]);
  isLoading = signal(true);
  errorMessage = signal<string>('');

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    // this.loadData(id);
  }

  // private loadData(id: string) {
  //   // Charger en parallèle l'entrée et les tags
  //   Promise.all([
  //     this.accountEnterService.getById(id).toPromise(),
  //     this.accountTagService.getAll().toPromise()
  //   ]).then(([accountEnter, tags]) => {
  //     if (accountEnter) {
  //       this.accountEnter.set(accountEnter);
  //     }
  //     if (tags) {
  //       this.tags.set(tags);
  //     }
  //     this.isLoading.set(false);
  //   }).catch(err => {
  //     console.error('Erreur lors du chargement', err);
  //     this.errorMessage.set('Impossible de charger l\'entrée de compte');
  //     this.isLoading.set(false);
  //   });
  // }

  // onUpdate(accountEnter: AccountEnter) {
  //   const id = this.route.snapshot.params['id'];
  //   this.accountEnterService.update(id, accountEnter).subscribe({
  //     next: () => {
  //       this.router.navigate(['/account-enters']);
  //     },
  //     error: (error) => {
  //       console.error('Erreur lors de la mise à jour', error);
  //       this.errorMessage.set('Erreur lors de la mise à jour de l\'entrée');
  //     }
  //   });
  // }

  onCancel() {
    this.router.navigate(['/account-enters']);
  }
}
