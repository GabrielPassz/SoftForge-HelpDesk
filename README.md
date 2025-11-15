# SoftForge HelpDesk: Sistema Inteligente de Gestão de Chamados

## 🚀 Visão Geral do Projeto

O **SoftForge HelpDesk** é uma solução acadêmica e inovadora desenvolvida para **modernizar e otimizar a gestão de chamados e suporte técnico** em ambientes corporativos. Criado como parte do Projeto Integrado Multidisciplinar (PIM IV) do curso de Análise e Desenvolvimento de Sistemas da UNIP, o sistema simula um ambiente de atendimento real, integrando recursos avançados de **Inteligência Artificial (IA)** para aumentar a eficiência operacional.

### ✨ Principais Funcionalidades

| Funcionalidade | Descrição | Benefício |
| --- | --- | --- |
| **Gestão de Chamados** | Abertura, acompanhamento e histórico detalhado de todas as solicitações. | Controle centralizado e rastreabilidade completa. |
| **IA Integrada** | Classificação automática de chamados e sugestão de soluções. | Redução do tempo de triagem e aumento da produtividade. |
| **Base de Conhecimento** | Repositório de soluções e artigos para autoatendimento. | Diminuição do volume de chamados repetitivos. |
| **Relatórios Gerenciais** | Dashboards e relatórios para análise de desempenho e tomada de decisão. | Visão estratégica sobre a operação de suporte. |
| **Controle de Acesso** | Perfis de usuário distintos: Solicitante, Técnico, Administrador e Gestor. | Segurança e delimitação de responsabilidades. |
| **Conformidade** | Implementação de políticas de segurança e aderência à **LGPD**. | Proteção de dados pessoais e rastreabilidade. |

## 🛠️ Tecnologias Utilizadas

O projeto foi construído com uma arquitetura robusta e moderna, focada em escalabilidade e segurança.

| Categoria | Tecnologia | Detalhes |
| --- | --- | --- |
| **Backend** | **ASP.NET MVC (C#)** | Framework robusto para a lógica de negócios e APIs. |
| **Banco de Dados** | **PostgreSQL** | Banco de dados relacional de código aberto, confiável e performático. |
| **Hospedagem DB** | **Supabase** | Plataforma de código aberto para hospedagem do PostgreSQL. |
| **Frontend** | **HTML, CSS, JavaScript** | Interface web responsiva, compatível com Web App (WA). |
| **Inteligência Artificial** | **Serviços de IA** | Utilizados para classificação e sugestão de soluções. |

## ⚙️ Instalação e Configuração

Para rodar o SoftForge HelpDesk localmente, siga os passos abaixo:

### Pré-requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) (Versão compatível com ASP.NET MVC)

- Servidor PostgreSQL (ou acesso a uma instância Supabase)

- Um editor de código (e.g., Visual Studio, VS Code)

### 1. Clonar o Repositório

```bash
git clone [URL_DO_SEU_REPOSITORIO]
cd SoftForge-HelpDesk
```

### 2. Configuração do Banco de Dados

1. Crie um banco de dados PostgreSQL.

1. Atualize a string de conexão no arquivo de configuração do projeto (`appsettings.json` ou similar) com suas credenciais do PostgreSQL/Supabase.

1. Execute as migrações do banco de dados para criar as tabelas (se aplicável, usando Entity Framework ou ferramenta similar).

### 3. Executar o Projeto

1. Restaure as dependências do projeto:

1. Execute a aplicação:

1. A aplicação estará acessível em `http://localhost:[PORTA]`.

## 👥 Autores

Este projeto foi desenvolvido por um grupo de estudantes do curso de Análise e Desenvolvimento de Sistemas da UNIP:

- **Gabriel Passos**

- **Andrey Vanolli**
- 
## 📄 Licença

Este projeto está licenciado sob uma **Licença Proprietária Temporária (All Rights Reserved — Academic Use Only)**.

O código-fonte é disponibilizado apenas para fins acadêmicos, avaliação do PIM/UNIP e portfólio pessoal. **Não é permitida a cópia, redistribuição, modificação ou uso comercial** sem autorização expressa dos autores.

A licença oficial e definitiva poderá ser definida futuramente conforme a continuidade do projeto.

Para detalhes completos, consulte o arquivo [LICENSE](LICENSE).

