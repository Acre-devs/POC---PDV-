export interface ItemVenda {
  produtoId: number;
  nomeProduto: string;
  precoUnitario: number;
  quantidade: number;
  subtotal: number;
}

export interface Venda {
  id: number;
  codigoVenda: string;
  dataCriacao: string;
  total: number;
  sincronizado: boolean;
  dataSincronizacao?: string;
  itens: ItemVenda[];
}

export interface StatusSincronizacao {
  servidorCentralOnline: boolean;
  vendasPendentesCount: number;
  ultimaSincronizacao?: string;
  mensagem: string;
}
