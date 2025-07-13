import { Component, OnInit } from '@angular/core';

// Imports do Angular Material
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Operacao } from './models/Operacao';
import { TopInvestidor } from './models/TopInvestidor';
import { DataService } from './services/data.service.ts';
import { finalize } from 'rxjs';
import { Posicao } from './models/Posicao';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatToolbarModule, MatCardModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatProgressSpinnerModule, MatIconModule,
    MatTableModule, CurrencyPipe, MatSelectModule, MatSnackBarModule
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App implements OnInit {
  // --- Modelos para os formulários ---
  consulta = { usuarioId: 1, ativoId: 1 };
  novaOperacao: Operacao = {
    usuarioId: 1,
    ativoId: 1,
    quantidade: 100,
    precoUnitario: 50.00,
    tipoOperacao: 'Compra',
    corretagem: 5.00
  };

  // --- Propriedades para exibir os resultados ---
  posicao: Posicao | null = null;
  precoMedio: number | null = null;
  ultimaCotacao: number | null = null;
  totalCorretagem: number | null = null;
  topPosicoes: TopInvestidor[] = [];
  topCorretagens: TopInvestidor[] = [];

  // --- Propriedades de estado da UI ---
  isLoading = false;
  error: string | null = null;

  // --- Configuração das tabelas de ranking ---
  displayedColumnsPosicao: string[] = ['usuarioId', 'valorTotal'];
  displayedColumnsCorretagem: string[] = ['usuarioId', 'totalCorretagem'];

  constructor(
    private dataService: DataService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.carregarRankings();
  }

  consultarTudo(): void {
    this.isLoading = true;
    this.error = null;
    this.resetarResultadosConsulta();

    this.dataService.getPosicao(this.consulta.usuarioId, this.consulta.ativoId).subscribe(data => this.posicao = data);
    this.dataService.getPrecoMedio(this.consulta.usuarioId, this.consulta.ativoId).subscribe(data => this.precoMedio = data);
    this.dataService.getUltimaCotacao(this.consulta.ativoId).subscribe(data => this.ultimaCotacao = data);
    this.dataService.getTotalCorretagem(this.consulta.usuarioId)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: data => this.totalCorretagem = data,
        error: err => this.handleError(err)
      });
  }

  carregarRankings(): void {
    this.dataService.getTop10PorPosicao().subscribe(data => this.topPosicoes = data);
    this.dataService.getTop10PorCorretagem().subscribe(data => this.topCorretagens = data);
  }

  submeterOperacao(): void {
    this.isLoading = true;
    this.error = null;
    this.dataService.criarOperacao(this.novaOperacao)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: () => {
          this.snackBar.open('Operação criada com sucesso!', 'Fechar', { duration: 3000 });
          // Atualiza os dados na tela após a operação
          this.consultarTudo();
          this.carregarRankings();
        },
        error: err => this.handleError(err)
      });
  }

  private resetarResultadosConsulta(): void {
    this.posicao = null;
    this.precoMedio = null;
    this.ultimaCotacao = null;
    this.totalCorretagem = null;
  }

  private handleError(err: any): void {
    console.error(err);
    this.error = `Ocorreu um erro ao comunicar com a API. Verifique se ela está a correr. (Erro: ${err.status})`;
    this.snackBar.open(this.error, 'Fechar', { duration: 5000, panelClass: 'error-snackbar' });
  }
}
