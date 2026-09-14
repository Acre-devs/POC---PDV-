import { Routes } from '@angular/router';
import { PdvComponent } from './components/pdv/pdv.component';
import { ProdutosComponent } from './components/produtos/produtos.component';

export const routes: Routes = [
  { path: '', redirectTo: 'pdv', pathMatch: 'full' },
  { path: 'pdv', component: PdvComponent },
  { path: 'produtos', component: ProdutosComponent },
  { path: '**', redirectTo: 'pdv' }
];
