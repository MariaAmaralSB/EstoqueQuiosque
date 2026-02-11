# EstoqueQuiosque

Aplicativo **.NET MAUI** para controle de estoque de um quiosque na praia, com foco em uso no **celular** e no **PC**.

## O que foi preparado

Foi criada uma base inicial de app MAUI com:

- Cadastro básico de produtos em memória.
- Tela principal para cadastro de produtos.
- Edição de produto existente (nome, unidade, quantidade, mínimo e custo).
- Registro de entradas e saídas de estoque.
- Indicador visual de itens abaixo do estoque mínimo.
- Histórico dos últimos movimentos.
- Contraste reforçado nas listas para melhorar visibilidade.
- Paleta revisada para melhor leitura (menos tons de azul e maior contraste).
- Estrutura em camadas simples (`Models`, `Services`, `ViewModels`, `Pages`) para facilitar evolução.
- Manifest Android criado em `Platforms/Android/AndroidManifest.xml` para evitar erro XA1018 em build local.
- Ajuste de empacotamento Windows (`WindowsPackageType=None`) para rodar sem AppxManifest durante desenvolvimento.
- Arquivos de inicialização por plataforma (Android/iOS/MacCatalyst/Windows) adicionados para evitar erro de entrada `Main` (CS5001).
- Removido `builder.Logging.AddDebug()` para eliminar erro de extensão de logging em ambientes sem pacote de debug (CS1061).

## Estrutura

```text
src/
  EstoqueQuiosque.App/
    Models/
    Services/
    ViewModels/
    Pages/
```

## Próximos passos recomendados

1. Instalar SDK do .NET 8 + workload MAUI:
   - `dotnet workload install maui`
2. Criar/adicionar pastas de plataforma (Android/Windows/iOS/MacCatalyst) caso não existam no seu template local.
3. Substituir armazenamento em memória por SQLite local (offline-first).
4. Adicionar autenticação de usuário (opcional para equipe).
5. Criar relatórios de vendas x estoque para reposição diária.

## Ideias específicas para quiosque de praia

- Sugestão automática de compra por sazonalidade (feriados e alta temporada).
- Alertas para produtos de alto giro (água, gelo, cerveja, protetor, snacks).
- Registro de perdas (quebra, vencimento, derretimento).
- Modo rápido de operação com botões grandes para uso sob sol.

## Observação

Neste ambiente, o comando `dotnet` não está disponível, então a validação de build não pôde ser executada aqui.
