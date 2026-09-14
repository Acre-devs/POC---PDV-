import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StatusSincronizacao, Venda } from '../models/venda.model';

@Injectable({
  providedIn: 'root'
})
export class SincronizacaoService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5001/api';

  getStatus(): Observable<StatusSincronizacao> {
    return this.http.get<StatusSincronizacao>(`${this.apiUrl}/status-sincronizacao`);
  }

  sincronizarAgora(): Observable<{ sincronizadas: number; mensagem: string }> {
    return this.http.post<{ sincronizadas: number; mensagem: string }>(`${this.apiUrl}/status-sincronizacao/sincronizar-agora`, {});
  }

  realizarVenda(venda: { codigoVenda?: string; total: number; itens: any[] }): Observable<Venda> {
    return this.http.post<Venda>(`${this.apiUrl}/vendas`, venda);
  }

  getVendasLocais(): Observable<Venda[]> {
    return this.http.get<Venda[]>(`${this.apiUrl}/vendas`);
  }

  getVendasConsolidadasCentral(): Observable<Venda[]> {
    return this.http.get<Venda[]>('http://localhost:5000/api/sincronizacao/vendas');
  }
}
