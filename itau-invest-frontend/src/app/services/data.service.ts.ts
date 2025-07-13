import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment'; // CORREÇÃO: Caminho ajustado para sair da pasta 'app' e 'services'
import { Posicao } from '../models/Posicao'; // CORREÇÃO: Caminho ajustado para a pasta 'models'
import { TopInvestidor } from '../models/TopInvestidor'; 
import { Operacao } from '../models/Operacao';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private apiUrl = environment.apiUrl; // Agora a URL base já é '.../api/investimentos'

  constructor(private http: HttpClient) { }

  // --- Métodos GET ---
  getPosicao(usuarioId: number, ativoId: number): Observable<Posicao> {
    return this.http.get<Posicao>(`${this.apiUrl}/posicao/${usuarioId}/${ativoId}`);
  }

  getPrecoMedio(usuarioId: number, ativoId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/preco-medio/${usuarioId}/${ativoId}`);
  }

  getUltimaCotacao(ativoId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/ultima-cotacao/${ativoId}`);
  }

  getTotalCorretagem(usuarioId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/corretagem-total/${usuarioId}`);
  }

  getTop10PorPosicao(): Observable<TopInvestidor[]> {
    return this.http.get<TopInvestidor[]>(`${this.apiUrl}/top10-posicao`);
  }

  getTop10PorCorretagem(): Observable<TopInvestidor[]> {
    return this.http.get<TopInvestidor[]>(`${this.apiUrl}/top10-corretagem`);
  }

  // --- Método POST ---
  criarOperacao(novaOperacao: Operacao): Observable<Operacao> {
    return this.http.post<Operacao>(`${this.apiUrl}/operacoes`, novaOperacao);
  }
}