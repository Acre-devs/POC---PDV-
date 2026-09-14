export interface Produto {
  id: number;
  nome: string;
  preco: number;
  ativo: boolean;
}

export interface CreateProduto {
  nome: string;
  preco: number;
  ativo?: boolean;
}

export interface ItemCarrinho {
  produto: Produto;
  quantidade: number;
  subtotal: number;
}
