import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProdutoService } from '../../services/produto.service';
import { Produto } from '../../models/produto.model';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './produtos.component.html',
  styleUrls: ['./produtos.component.css']
})
export class ProdutosComponent implements OnInit {
  private produtoService = inject(ProdutoService);

  produtos: Produto[] = [];
  carregando: boolean = true;
  salvando: boolean = false;

  novoNome: string = '';
  novoPreco: number | null = null;
  novoAtivo: boolean = true;

  mensagemSucesso: string | null = null;
  mensagemErro: string | null = null;

  ngOnInit(): void {
    this.carregarProdutos();
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
        this.mensagemErro = 'Não foi possível carregar os produtos.';
        this.carregando = false;
      }
    });
  }

  cadastrarProduto(): void {
    this.mensagemSucesso = null;
    this.mensagemErro = null;

    if (!this.novoNome.trim()) {
      this.mensagemErro = 'Informe o nome do produto.';
      return;
    }

    if (this.novoPreco === null || this.novoPreco <= 0) {
      this.mensagemErro = 'Informe um preço válido maior que zero.';
      return;
    }

    this.salvando = true;
    this.produtoService.createProduto({
      nome: this.novoNome.trim(),
      preco: this.novoPreco,
      ativo: this.novoAtivo
    }).subscribe({
      next: (produtoCriado) => {
        this.salvando = false;
        this.mensagemSucesso = `Produto "${produtoCriado.nome}" cadastrado com sucesso! (ID: ${produtoCriado.id})`;
        this.novoNome = '';
        this.novoPreco = null;
        this.novoAtivo = true;
        this.carregarProdutos();
      },
      error: (err) => {
        console.error('Erro ao cadastrar produto', err);
        this.salvando = false;
        this.mensagemErro = 'Erro ao salvar o produto na API.';
      }
    });
  }
}
