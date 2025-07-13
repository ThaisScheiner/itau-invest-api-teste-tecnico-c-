# Itaú Investimentos - API de Posição de Clientes (Teste Técnico)

## Visão Geral

Este projeto é uma solução de backend desenvolvida como parte de um teste técnico compartilhado no Linkedin por um coordenador de TI, então decidi desenvolver como aprendizado para possíveis vagas. 

A aplicação simula um sistema de controle de investimentos em renda variável, processando operações e cotações de mercado para calcular e expor a posição consolidada dos clientes em tempo real.

A arquitetura foi projetada para ser robusta, escalável e reativa, utilizando uma abordagem orientada a eventos com **.NET 8**, **Docker**, **MySQL** e **Apache Kafka**.

## Arquitetura da Solução

O sistema é composto por uma API central e serviços de background que operam de forma assíncrona, garantindo que a aplicação principal permaneça responsiva enquanto processa eventos de mercado.

1.  **API REST (.NET 8):** O núcleo da aplicação. Responsável por:
    * Expor endpoints `RESTful` para consulta de dados (posições, preço médio, rankings, etc.).
    * Receber novas operações (compras/vendas) e publicá-las para processamento.
    * Servir como ponto de entrada para interações do usuário.

2.  **Workers de Mensageria (Kafka Consumers):** Serviços que rodam em segundo plano e escutam eventos do Apache Kafka, permitindo o processamento desacoplado e em tempo real.
    * **`CotacaoWorker`**: Consome o tópico `cotacoes`. Ao receber uma nova cotação de mercado, ele a salva no banco de dados e dispara o recálculo do Lucro/Prejuízo (P&L) de todas as posições impactadas.
    * **`PosicaoWorker`**: Consome o tópico `operacoes-novas`. Ao receber uma nova operação de compra ou venda, ele recalcula a posição consolidada do cliente (quantidade e preço médio) para aquele ativo.

3.  **Infraestrutura Containerizada (Docker):** Todos os serviços de infraestrutura (banco de dados e mensageria) são gerenciados pelo Docker Compose, garantindo um ambiente de desenvolvimento consistente, portátil e fácil de configurar.

## Tecnologias Utilizadas

| Tecnologia              | Propósito                                                                                                                                                           |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **.NET 8 / C#** | Plataforma principal para o desenvolvimento do backend, oferecendo alta performance, robustez e um ecossistema moderno.                                               |
| **ASP.NET Core** | Framework para a construção da API REST, fornecendo uma base sólida para os endpoints.                                                                                |
| **Entity Framework Core** | ORM utilizado para o mapeamento objeto-relacional e comunicação com o banco de dados.                                                                               |
| **MySQL** | Banco de dados relacional escolhido para persistir todos os dados da aplicação (usuários, ativos, operações, etc.).                                                   |
| **Apache Kafka** | Plataforma de streaming de eventos utilizada para a comunicação assíncrona entre os serviços, garantindo desacoplamento e escalabilidade.                              |
| **Docker & Docker Compose** | Ferramentas para criar, gerenciar e orquestrar os contêineres da infraestrutura (MySQL, Kafka, Zookeeper, Kafka UI), garantindo um ambiente de desenvolvimento consistente. |
| **Swagger (OpenAPI)** | Ferramenta para documentação e teste interativo dos endpoints da API, gerada automaticamente pela aplicação.                                                          |
| **xUnit (Próximo Passo)** | Framework de testes unitários para garantir a qualidade e a corretude da lógica de negócio.                                                                          |

## Como Executar o Projeto

Siga os passos abaixo para configurar e rodar a aplicação completa em seu ambiente local.

### Pré-requisitos

* **Docker Desktop:** [Instale aqui](https://www.docker.com/products/docker-desktop/)
* **.NET 8 SDK:** [Instale aqui](https://dotnet.microsoft.com/download/dotnet/8.0)
* **Git:** [Instale aqui](https://git-scm.com/downloads)

### Passo 1: Clonar o Repositório

```bash
git clone <URL_DO_SEU_REPOSITORIO>
cd <NOME_DA_PASTA_DO_PROJETO>
```

### Passo 2: Iniciar a Infraestrutura com Docker

Este comando irá baixar as imagens e iniciar todos os serviços de infraestrutura (MySQL, Zookeeper, Kafka, etc.) em segundo plano.

```bash
docker-compose up -d
```

Aguarde cerca de 1 a 2 minutos para que todos os serviços, especialmente o Kafka, fiquem totalmente saudáveis. Você pode verificar o status com o comando `docker ps`.

### Passo 3: Iniciar a Aplicação .NET

Com a infraestrutura no ar, inicie a API.

1.  Navegue até a pasta do projeto da API:
    ```bash
    cd ItauInvest.API
    ```
2.  Execute o comando:
    ```bash
    dotnet run
    ```

### Passo 4: Acessar as Aplicações

* **API (Swagger):** Abra seu navegador e acesse a URL que apareceu no terminal do `dotnet run` (geralmente `http://localhost:5228` ou `https://localhost:7095`).
* **Kafka UI (Interface Visual):** Para ver os tópicos e mensagens, acesse `http://localhost:8080`.

## Testando a Aplicação

### Teste 1: Fluxo de Cotações

1.  Acesse o **Kafka UI** (`http://localhost:8080`).
2.  Navegue até o tópico **`cotacoes`**.
3.  Produza uma nova mensagem com o seguinte JSON:
    ```json
    {
      "CodigoAtivo": "PETR4",
      "Preco": 39.50
    }
    ```
4.  Observe o terminal da aplicação .NET. O `CotacaoWorker` irá processar a mensagem e recalcular as posições.

### Teste 2: Fluxo de Operações

1.  Acesse a documentação **Swagger** da sua API.
2.  Use o endpoint `POST /api/investimentos/operacoes` para criar uma nova operação de compra para um usuário e ativo existentes.
3.  Use o endpoint `GET /api/investimentos/posicao/{usuarioId}/{ativoId}` para verificar se a quantidade e o preço médio do cliente foram atualizados corretamente.

## Estrutura do Projeto

```
/
├── ItauInvest.API/
│   ├── Domain/             # Contém as entidades do negócio (POCOs)
│   ├── Application/
│   │   ├── Services/       # Contém a lógica de negócio (PosicaoService, etc.)
│   │   └── Workers/        # Contém os consumidores Kafka
│   ├── Infrastructure/
│   │   └── Data/           # Contém o DbContext e as configurações do EF Core
│   ├── Controllers/        # Contém os endpoints da API REST
│   ├── Middleware/         # Contém middlewares customizados (ex: tratamento de erro)
│   └── Program.cs          # Ponto de entrada e configuração da aplicação
├── docker-compose.yml      # Orquestração dos contêineres da infraestrutura
└── create-topics.sh        # Script para criação automática dos tópicos Kafka
