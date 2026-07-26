import { ResolveFn } from '@angular/router';
import { PagedUtilisateur, Utilisateur } from '../../../../core/utilisateurs/utilisateur.model';
import { inject } from '@angular/core';
import { UtilisateurService } from '../../../../core/utilisateurs/utilisateur.service';

export const utilisateurResolver:ResolveFn<PagedUtilisateur>=(route,state)=>{
    return inject(UtilisateurService).GetUtilisateur();
}
