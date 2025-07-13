export interface Operacao {
  usuarioId: number;
  ativoId: number;
  quantidade: number;
  precoUnitario: number;
  tipoOperacao: 'Compra' | 'Venda';
  corretagem: number;
}