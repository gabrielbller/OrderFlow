# OrderFlow — Sistema de Gestão de Pedidos de E-commerce

Projeto de Disciplina — **Tecnologia .NET** (Pós-Graduação)
Autor: **Gabriel Bller**

Aplicação de domínio modelada com **Domain-Driven Design (DDD)**, implementada em **C#** com
foco em **Orientação a Objetos**, princípios **SOLID/GRASP** e **testes unitários** com cobertura
de domínio superior a 80%.

> 📄 **Documento explicativo do projeto:** o arquivo
> [`GabrielBodenmuller_TecnologiaNET_pd.pdf`](GabrielBodenmuller_TecnologiaNET_pd.pdf) descreve o problema,
> o escopo, as regras de negócio, os usuários do sistema e a modelagem do domínio (com diagramas,
> Context Map e o mapeamento detalhado de cada item da rubrica).

---

## 🛠️ Pré-requisitos

- **.NET SDK 10.0** (os projetos têm como alvo `net10.0`)

Verifique com:
```bash
dotnet --version
```

## ▶️ Como rodar

Na raiz do projeto (pasta `OrderFlow/`, onde fica o `OrderFlow.sln`):

```bash
# Restaurar dependências
dotnet restore

# Compilar a solução inteira
dotnet build

# Executar os testes unitários (118 testes)
dotnet test
```

### Relatório de cobertura do domínio

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Include="[Domain]*"
```

Resultado atual:

| Métrica  | Cobertura            |
|----------|----------------------|
| Linhas   | **89,25%** (465/521) |
| Branches | 76,89% (183/238)     |
| Métodos  | 81,02%               |

> A meta da rubrica (> 80% de linhas no domínio) é **superada**.

---

## 🗂️ Estrutura do projeto

```
OrderFlow/
├── OrderFlow.sln
├── GabrielBller_TecnologiaNET_pd.pdf   # Documento explicativo (entrega)
├── src/
│   ├── Domain/            # Núcleo do domínio (DDD) — alvo dos testes de cobertura
│   ├── Application/       # Casos de uso / orquestração (GRASP Controller)
│   └── Infrastructure/    # Repositórios em memória + Anti-Corruption Layer
└── tests/
    └── Domain.Tests/      # Testes unitários (xUnit + Moq)
```

O domínio está organizado em **4 Bounded Contexts**: `Customer`, `Inventory`,
`OrderManagement` e `Payment`.

---

## ✅ Onde cada item da rubrica está implementado

### Parte 1 — Domain-Driven Design

| Conceito DDD | Onde encontrar |
|---|---|
| **Ubiquitous Language** | Nomes do domínio em todo `src/Domain` (Order, Customer, Product, Reserve/ReleaseStock, ConfirmOrder...) |
| **Entities** | [Customer.cs](src/Domain/Customer/Entities/Customer.cs), [Product.cs](src/Domain/Inventory/Entities/Product.cs), [Order.cs](src/Domain/OrderManagement/Entities/Order.cs), [OrderItem.cs](src/Domain/OrderManagement/Entities/OrderItem.cs) |
| **Value Objects** | [Money.cs](src/Domain/OrderManagement/ValueObjects/Money.cs), [Address.cs](src/Domain/OrderManagement/ValueObjects/Address.cs), [OrderStatus.cs](src/Domain/OrderManagement/ValueObjects/OrderStatus.cs), [Cpf.cs](src/Domain/Customer/ValueObjects/Cpf.cs), [Email.cs](src/Domain/Customer/ValueObjects/Email.cs), [ProductCode.cs](src/Domain/Inventory/ValueObjects/ProductCode.cs), [PaymentResult.cs](src/Domain/Payment/ValueObjects/PaymentResult.cs) |
| **Repositories** | [ICustomerRepository.cs](src/Domain/Customer/Repositories/ICustomerRepository.cs), [IProductRepository.cs](src/Domain/Inventory/Repositories/IProductRepository.cs), [IOrderRepository.cs](src/Domain/OrderManagement/Repositories/IOrderRepository.cs) |
| **Aggregate / Aggregate Root** | [Order.cs](src/Domain/OrderManagement/Entities/Order.cs) (root de Order + OrderItem), base [AggregateRoot.cs](src/Domain/Common/AggregateRoot.cs) |
| **Bounded Contexts** | Pastas `Customer/`, `Inventory/`, `OrderManagement/`, `Payment/` em [src/Domain](src/Domain) |
| **Domain Services** | [OrderDomainService.cs](src/Domain/OrderManagement/Services/OrderDomainService.cs), [InventoryDomainService.cs](src/Domain/Inventory/Services/InventoryDomainService.cs) |
| **Factory** (diferenciada de Domain Service) | [OrderFactory.cs](src/Domain/OrderManagement/Factories/OrderFactory.cs) — cabeçalho explica a diferença Factory × Domain Service |
| **Anti-Corruption Layer** | [PaymentServiceAdapter.cs](src/Infrastructure/AntiCorruptionLayer/PaymentServiceAdapter.cs) + [ExternalPaymentDto.cs](src/Infrastructure/AntiCorruptionLayer/ExternalPaymentDto.cs) |
| **Context Map** | Diagrama e descrição no [PDF](GabrielBller_TecnologiaNET_pd.pdf) |

### Parte 2 — Orientação a Objetos com C#

| Conceito OO | Onde encontrar |
|---|---|
| **Encapsulamento** | Setters privados e construtores `internal` em [Order.cs](src/Domain/OrderManagement/Entities/Order.cs) e [OrderItem.cs](src/Domain/OrderManagement/Entities/OrderItem.cs); coleção exposta como `IReadOnlyList` |
| **Abstração** | Bases abstratas [Entity.cs](src/Domain/Common/Entity.cs) e [ValueObject.cs](src/Domain/Common/ValueObject.cs); `Total`/`Subtotal` calculados, não armazenados |
| **Herança** | Hierarquia de 3 níveis: `Entity` → [AggregateRoot.cs](src/Domain/Common/AggregateRoot.cs) → `Order`/`Product`/`Customer` |
| **Polimorfismo** | `override` de `GetEqualityComponents`/`Equals` nos Value Objects e Entidades; método abstrato em [ValueObject.cs](src/Domain/Common/ValueObject.cs) |
| **Modificadores de acesso, propriedades, métodos e construtores** | Uso de `public`/`private`/`protected`/`internal` por toda a `src/Domain` |

### Parte 3 — SOLID e GRASP

> Cada princípio está **destacado em comentários** no topo e ao longo das classes
> (`// SOLID - SRP`, `// GRASP - Low Coupling`, etc.), com nome, objetivo e explicação.

| Princípio / Padrão | Onde encontrar |
|---|---|
| **SRP** (Single Responsibility) | [Entity.cs](src/Domain/Common/Entity.cs), [Cpf.cs](src/Domain/Customer/ValueObjects/Cpf.cs) e cada classe do domínio |
| **OCP** (Open/Closed) | [ValueObject.cs](src/Domain/Common/ValueObject.cs), [OrderStatus.cs](src/Domain/OrderManagement/ValueObjects/OrderStatus.cs) |
| **DIP** (Dependency Inversion) | Injeção de interfaces em [OrderDomainService.cs](src/Domain/OrderManagement/Services/OrderDomainService.cs) e [OrderFactory.cs](src/Domain/OrderManagement/Factories/OrderFactory.cs) |
| **Low Coupling** (GRASP) | [OrderDomainService.cs](src/Domain/OrderManagement/Services/OrderDomainService.cs) — depende só de abstrações |
| **High Cohesion** (GRASP) | [InventoryDomainService.cs](src/Domain/Inventory/Services/InventoryDomainService.cs) |
| **Controller** (GRASP) | [OrderApplicationService.cs](src/Application/Orders/OrderApplicationService.cs) — recebe e coordena os casos de uso, sem regras de negócio |
| **Creator** (GRASP) | [OrderFactory.cs](src/Domain/OrderManagement/Factories/OrderFactory.cs) |

### Parte 4 — Testes Unitários e TDD

| Item | Onde encontrar |
|---|---|
| **Princípios F.I.R.S.T.** (isolamento, repetibilidade, rapidez, auto-verificação, abrangência) | Comentados em [OrderDomainServiceTests.cs](tests/Domain.Tests/OrderDomainServiceTests.cs) |
| **Testes de regras de negócio** | [OrderTests.cs](tests/Domain.Tests/OrderTests.cs), [ProductTests.cs](tests/Domain.Tests/ProductTests.cs), [CustomerTests.cs](tests/Domain.Tests/CustomerTests.cs), [MoneyTests.cs](tests/Domain.Tests/MoneyTests.cs), [OrderStatusTests.cs](tests/Domain.Tests/OrderStatusTests.cs), [AddressTests.cs](tests/Domain.Tests/AddressTests.cs) |
| **Mocks e Stubs** (Moq) | [OrderDomainServiceTests.cs](tests/Domain.Tests/OrderDomainServiceTests.cs), [OrderFactoryTests.cs](tests/Domain.Tests/OrderFactoryTests.cs), [InventoryDomainServiceTests.cs](tests/Domain.Tests/InventoryDomainServiceTests.cs) |
| **Testes negativos** | Seções "TESTES NEGATIVOS" em cada classe de teste |
| **Cobertura > 80%** | **89,25%** de linhas do domínio (ver comando acima) |

---

## 📦 Tecnologias

- C# / .NET 10
- xUnit (testes)
- Moq (mocks/stubs)
- coverlet (cobertura)

## 🔗 Repositório

https://github.com/gabrielbller/OrderFlow
