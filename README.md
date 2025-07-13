# Investimentos - API de Posição de Clientes (Teste Técnico)

## Visão Geral

Este projeto é uma solução completa desenvolvida como parte do processo seletivo compartilhada no Linkedin por um coordenador de TI do Itaú, então resolvi desenvolver essa aplicação como aprendizado para futuras vagas para que eu possa me candidatar. A aplicação simula um sistema de controlo de investimentos em renda variável, processando operações e cotações de mercado para calcular e expor a posição consolidada dos clientes em tempo real, com um frontend interativo para visualização.

A arquitetura foi projetada para ser robusta, escalável e reativa, utilizando uma abordagem orientada a eventos com **.NET 8**, **Angular Standalone**, **Docker**, **MySQL** e **Apache Kafka**.

## Arquitetura da Solução

O sistema é composto por uma API central e serviços de background que operam de forma assíncrona, garantindo que a aplicação principal permaneça responsiva enquanto processa eventos de mercado.

1.  **API REST (.NET 8):** O núcleo da aplicação. Responsável por:
    * Expor endpoints `RESTful` para consulta de dados (posições, preço médio, rankings, etc.).
    * Receber novas operações (compras/vendas) e publicá-las para processamento.
    * Servir como ponto de entrada para interações do utilizador.

2.  **Workers de Mensageria (Kafka Consumers):** Serviços que correm em segundo plano e escutam eventos do Apache Kafka, permitindo o processamento desacoplado e em tempo real.
    * **`CotacaoWorker`**: Consome o tópico `cotacoes`. Ao receber uma nova cotação de mercado, ele guarda-a na base de dados e dispara o recálculo do Lucro/Prejuízo (P&L) de todas as posições impactadas.
    * **`PosicaoWorker`**: Consome o tópico `operacoes-novas`. Ao receber uma nova operação de compra ou venda, ele recalcula a posição consolidada do cliente (quantidade e preço médio) para aquele ativo.

3.  **Infraestrutura Containerizada (Docker):** Todos os serviços de infraestrutura (base de dados e mensageria) são geridos pelo Docker Compose, garantindo um ambiente de desenvolvimento consistente, portátil e fácil de configurar.

4.  **Frontend (Angular Standalone):** Uma interface de utilizador moderna e reativa que consome a API .NET para apresentar os dados de forma visual, permitindo a consulta de posições e a criação de novas operações.

## Tecnologias Utilizadas

| **Tecnologia** | **Propósito** |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **.NET 8 / C#** | Plataforma principal para o desenvolvimento do backend, oferecendo alta performance, robustez e um ecossistema moderno.                                                 |
| **ASP.NET Core** | Framework para a construção da API REST, fornecendo uma base sólida para os endpoints.                                                                                  |
| **Angular Standalone** | Framework frontend para a criação de uma Single-Page Application (SPA) moderna, reativa e com uma excelente experiência de utilizador.                                   |
| **Entity Framework Core** | ORM utilizado para o mapeamento objeto-relacional e comunicação com a base de dados.                                                                                 |
| **MySQL** | Base de dados relacional escolhida para persistir todos os dados da aplicação (utilizadores, ativos, operações, etc.).                                                     |
| **Apache Kafka** | Plataforma de streaming de eventos utilizada para a comunicação assíncrona entre os serviços, garantindo desacoplamento e escalabilidade.                                |
| **Docker & Docker Compose** | Ferramentas para criar, gerir e orquestrar os contentores da infraestrutura (MySQL, Kafka, Zookeeper, Kafka UI), garantindo um ambiente de desenvolvimento consistente. |
| **Swagger (OpenAPI)** | Ferramenta para documentação e teste interativo dos endpoints da API, gerada automaticamente pela aplicação.                                                            |

## Como Executar o Projeto

Siga os passos abaixo para configurar e correr a aplicação completa no seu ambiente local.

### Pré-requisitos

* **Docker Desktop:** [Instale aqui](https://www.docker.com/products/docker-desktop/)
* **.NET 8 SDK:** [Instale aqui](https://dotnet.microsoft.com/download/dotnet/8.0)
* **Node.js e Angular CLI:** [Instale aqui](https://angular.io/cli)
* **Git:** [Instale aqui](https://git-scm.com/downloads)

### Passo 1: Clonar o Repositório

```bash
git clone <URL_DO_SEU_REPOSITORIO>
cd <NOME_DA_PASTA_DO_PROJETO>
```

### Passo 2: Iniciar a Infraestrutura com Docker

Este comando irá descarregar as imagens e iniciar todos os serviços de infraestrutura (MySQL, Zookeeper, Kafka, etc.) em segundo plano.

```bash
docker-compose up -d
```

Aguarde cerca de 1 a 2 minutos para que todos os serviços, especialmente o Kafka, fiquem totalmente saudáveis. Pode verificar o estado com o comando `docker ps`.

### Passo 3: Iniciar a Aplicação Backend (.NET)

Num **novo terminal**, navegue até à pasta do projeto da API e execute-a.

1.  Navegue até à pasta:
    ```bash
    cd ItauInvest.API
    ```
2.  Execute o comando:
    ```bash
    dotnet run
    ```

### Passo 4: Iniciar a Aplicação Frontend (Angular)

Num **terceiro terminal**, navegue até à pasta do projeto frontend e execute-o.

1.  Navegue até à pasta:
    ```bash
    cd itau-invest-frontend
    ```
2.  Execute o comando:
    ```bash
    ng serve
    ```

### Passo 5: Aceder às Aplicações

* **Frontend (Aplicação Principal):** Abra o seu navegador e aceda a `http://localhost:4200`.
* **API (Swagger):** Para testar a API diretamente, aceda à URL que apareceu no terminal do `dotnet run` (geralmente `http://localhost:5228`).
* **Kafka UI (Interface Visual):** Para ver os tópicos e mensagens, aceda a `http://localhost:8080`.

## Estrutura do Projeto

```
/ (Raiz do Repositório)
├── ItauInvest.API/          # Pasta do projeto Backend .NET
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/
│   └── ...
├── itau-invest-frontend/    # Pasta do projeto Frontend Angular
│   ├── src/
│   └── ...
├── ItauInvest.API.sln       # Ficheiro da Solution .NET
├── docker-compose.yml       # Orquestração dos contentores da infraestrutura
└── README.md
