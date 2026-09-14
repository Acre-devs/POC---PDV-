import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProdutoService } from '../../services/produto.service';
import { SincronizacaoService } from '../../services/sincronizacao.service';
import { Produto, ItemCarrinho } from '../../models/produto.model';
import { StatusSincronizacao, Venda } from '../../models/venda.model';

@Component({
  selector: 'app-pdv',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pdv.component.html',
  styleUrls: ['./pdv.component.css']
})
export class PdvComponent implements OnInit, OnDestroy {
  private produtoService = inject(ProdutoService);
  private syncService = inject(SincronizacaoService);

  produtos: Produto[] = [];
  carrinho: ItemCarrinho[] = [];
  termoBusca: string = '';
  carregando: boolean = true;

  statusSync: StatusSincronizacao = {
    servidorCentralOnline: false,
    vendasPendentesCount: 0,
    mensagem: 'Verificando status do Servidor Central...'
  };

  sincronizandoManual: boolean = false;
  mensagemFeedback: string | null = null;
  vendaFinalizadaSucesso: Venda | null = null;

  private pollInterval: any;

  ngOnInit(): void {
    this.carregarStatusSync();
    this.carregarProdutos();

    this.pollInterval = setInterval(() => {
      this.carregarStatusSync();
    }, 3000);
  }

  ngOnDestroy(): void {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
    }
  }

  carregarStatusSync(): void {
    this.syncService.getStatus().subscribe({
      next: (res) => {
        this.statusSync = res;
      },
      error: () => {
        this.statusSync = {
          servidorCentralOnline: false,
          vendasPendentesCount: this.statusSync.vendasPendentesCount,
          mensagem: 'MODO CONTINGÊNCIA: Servidor Central Offline.'
        };
      }
    });
  }

  carregarProdutos(): void {
    this.carregando = true;
    this.produtoService.getProdutos().subscribe({
      next: (data) => {
        this.produtos = data;
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao carregar produtos', err);
        this.carregando = false;
        this.exibirFeedback('Erro ao conectar com a API para buscar produtos.');
      }
    });
  }

  get produtosFiltrados(): Produto[] {
    if (!this.termoBusca.trim()) {
      return this.produtos.filter(p => p.ativo);
    }
    const termo = this.termoBusca.toLowerCase();
    return this.produtos.filter(p => p.ativo && p.nome.toLowerCase().includes(termo));
  }

  adicionarAoCarrinho(produto: Produto): void {
    const itemExistente = this.carrinho.find(i => i.produto.id === produto.id);
    if (itemExistente) {
      itemExistente.quantidade += 1;
      itemExistente.subtotal = itemExistente.quantidade * itemExistente.produto.preco;
    } else {
      this.carrinho.push({
        produto: produto,
        quantidade: 1,
        subtotal: produto.preco
      });
    }
    this.vendaFinalizadaSucesso = null;
  }

  alterarQuantidade(item: ItemCarrinho, delta: number): void {
    item.quantidade += delta;
    if (item.quantidade <= 0) {
      this.removerItem(item);
    } else {
      item.subtotal = item.quantidade * item.produto.preco;
    }
  }

  removerItem(item: ItemCarrinho): void {
    this.carrinho = this.carrinho.filter(i => i.produto.id !== item.produto.id);
  }

  limparCarrinho(): void {
    this.carrinho = [];
  }

  get totalCarrinho(): number {
    return this.carrinho.reduce((acc, item) => acc + item.subtotal, 0);
  }

  get totalItens(): number {
    return this.carrinho.reduce((acc, item) => acc + item.quantidade, 0);
  }

  finalizarVenda(): void {
    if (this.carrinho.length === 0) {
      this.exibirFeedback('O carrinho está vazio! Adicione produtos antes de finalizar.');
      return;
    }

    const payload = {
      total: this.totalCarrinho,
      itens: this.carrinho.map(i => ({
        produtoId: i.produto.id,
        nomeProduto: i.produto.nome,
        precoUnitario: i.produto.preco,
        quantidade: i.quantidade,
        subtotal: i.subtotal
      }))
    };

    this.syncService.realizarVenda(payload).subscribe({
      next: (vendaCriada) => {
        this.vendaFinalizadaSucesso = vendaCriada;
        this.carrinho = [];
        this.carregarStatusSync();
        this.exibirFeedback(`Venda ${vendaCriada.codigoVenda} registrada com sucesso na contingência local!`);
      },
      error: (err) => {
        console.error('Erro ao finalizar venda', err);
        this.exibirFeedback('Erro ao gravar venda no banco local do Caixa.');
      }
    });
  }

  forcarSincronizacao(): void {
    this.sincronizandoManual = true;
    this.syncService.sincronizarAgora().subscribe({
      next: (res) => {
        this.sincronizandoManual = false;
        this.carregarStatusSync();
        this.exibirFeedback(res.mensagem);
      },
      error: () => {
        this.sincronizandoManual = false;
        this.exibirFeedback('Não foi possível conectar ao Servidor Central para sincronizar.');
      }
    });
  }

  exibirFeedback(msg: string): void {
    this.mensagemFeedback = msg;
    setTimeout(() => {
      if (this.mensagemFeedback === msg) {
        this.mensagemFeedback = null;
      }
    }, 5000);
  }
}
