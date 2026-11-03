# OpenClevoRGB (Clevo / Avell N850HP6 / N870HP6)

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D6.svg)
![Framework](https://img.shields.io/badge/.NET-4.8%20%7C%20C%23-512BD4.svg)
![Size](https://img.shields.io/badge/app%20size-~39%20KB-success.svg)
![AI](https://img.shields.io/badge/AI%20Pair%20Programmer-Gemini%203.8%20Flash-4285F4?logo=google&logoColor=white)

Utilitário nativo, ultraleve e moderno para controle de cores, luminosidade e efeitos do teclado retroiluminado RGB de notebooks **Clevo** e marcas parceiras como **Avell**, **Sager**, **Hasee** e **Schenker** (baseados na plataforma Clevo N85_87HP6 e compatíveis).

---

## 🚀 Destaques e Vantagens

- **Ultra-leve:** Executável compilado de apenas **~39 KB** e consumo de memória de apenas **~12 MB de RAM** (praticamente invisível para jogos e tarefas pesadas).
- **Instalador 100% Independente:** Arquivo único `bin\Instalador_OpenClevoRGB.exe` (~73 KB) que possui todos os binários embutidos internamente. Pode ser baixado e executado isoladamente em qualquer pasta (ex: Downloads ou Desktop) sem depender de nenhum outro arquivo.
- **Script Transparente Open Source:** Para quem prefere inspecionar o código antes de rodar, o script aberto `instalar.bat` realiza todo o processo de forma transparente em lote.
- **Inicialização Automática Confiável:** Utiliza o **Agendador de Tarefas do Windows (`Task Scheduler`)** com privilégios elevados (`/rl highest`), contornando o bloqueio de UAC do Windows e iniciando silenciosamente junto com o sistema no boot.
- **Atalhos de Teclado Globais & Teclas Físicas do Notebook:**
  - Suporte às combinações padrão com ícones impressos no teclado numérico Clevo / Avell:
    - **Trocar Cores (Ciclo):** `Fn + /` ou `Ctrl + Num /`
    - **Ligar / Desligar Luz:** `Fn + *` ou `Ctrl + Num *`
    - **Aumentar Brilho (+):** `Fn + +` ou `Ctrl + Num +`
    - **Reduzir Brilho (-):** `Fn + -` ou `Ctrl + Num -`
  - **Captura Híbrida Inteligente:** Escuta eventos de hardware ACPI em segundo plano (`CLEVO_EVENT` via WMI) e registra atalhos globais no Windows (`RegisterHotKey`) que funcionam mesmo enquanto você joga ou utiliza qualquer aplicativo em tela cheia com o utilitário minimizado na bandeja.
  - **Totalmente Personalizável:** Tela dedicada para customizar cada atalho com qualquer modificador (`Ctrl`, `Alt`, `Shift`, `Win`) e tecla de sua preferência.
- **Restauração Automática Pós-Suspensão (Sleep/Resume):** Monitora eventos de energia e restaura as cores salvas após o notebook acordar da suspensão.
- **Controle de 3 Zonas RGB Independentes:**
  - **Zona Esquerda** *(QWE / ASD)*
  - **Zona Central** *(TYU / GHJ)*
  - **Zona Direita** *(Teclado Numérico)*
  - **Modo Sincronizado:** Aplica a mesma cor em todas as zonas simultaneamente.
- **Controle de Luminosidade:** Slider contínuo + 5 botões de atalho rápido:
  `[ Off (0%) ]` • `[ 25% ]` • `[ 50% ]` • `[ 75% ]` • `[ 100% ]`
- **Efeitos de Hardware da BIOS:** Estático, Respiração (*Breathe*), Ciclo (*Cycle*), Onda (*Wave*), Dança (*Dance*), Flash e Aleatório.
- **Dicas no Cursor (Tooltips):** Textos concisos e informativos exibidos em Dark Mode ao passar o mouse por cima (Hover).
- **Minimiza para a Bandeja (System Tray):** Fica recolhido perto do relógio com menu de acesso rápido pelo botão direito.

---

## 📦 Métodos de Instalação

### Opção 1: Instalador Gráfico Independente (Recomendado)
Ideal para a maioria dos usuários ou para download direto via **GitHub Releases**:
1. Baixe ou execute **`bin\Instalador_OpenClevoRGB.exe`**.
2. Escolha o diretório de destino (padrão: `C:\Program Files\OpenClevoRGB`).
3. Marque as opções desejadas (atalho no Desktop, Menu Iniciar, iniciar no boot).
4. Clique em **"🚀 Instalar Agora"**.

> *Nota: O instalador é 100% standalone e autossuficiente — ele já contém o aplicativo, desinstalador e os drivers embutidos internamente.*

### Opção 2: Script Transparente (Para Auditoria / Open Source)
Para usuários que não desejam rodar o instalador executável e preferem ler e inspecionar cada comando executado:
1. Clique com o botão direito no arquivo **`instalar.bat`** (na raiz) ou em **`scripts\instalar.bat`**.
2. Selecione **"Executar como Administrador"**.
3. O script criará a pasta oficial em Arquivos de Programas, copiará os arquivos, registrará o driver ACPI, criará os atalhos e configurará a inicialização automática no boot.

### Opção 3: Uso Portátil / Sem Instalação
1. Execute diretamente **`bin\OpenClevoRGB.exe`** como Administrador.

---

## 🗑️ Como Desinstalar

- **Via Painel de Controle / Configurações do Windows:**  
  Vá em *Configurações do Windows > Aplicativos > Aplicativos Instalados* (ou *Painel de Controle > Programas e Recursos*), localize **OpenClevoRGB** e clique em **Desinstalar**.
- **Via Script:**  
  Execute **`scripts\desinstalar.bat`** como Administrador.

---

## 📁 Estrutura do Projeto

```
OpenClevoRGB/
│
├── bin/                             # Executáveis finais prontos para distribuição
│   ├── Instalador_OpenClevoRGB.exe  # Instalador gráfico independente (~89 KB)
│   ├── OpenClevoRGB.exe             # Aplicativo principal standalone (~39 KB)
│   ├── Desinstalador.exe            # Desinstalador gráfico dedicado (~15 KB)
│   └── clevomof.dll                 # Biblioteca MOF ACPI necessária para o hardware
│
├── resources/                       # Recursos de sistema e manifesto
│   ├── app.manifest                 # Manifesto do Windows (DPI PerMonitorV2 + requireAdministrator)
│   ├── app.ico                      # Ícone oficial de alta resolução do aplicativo
│   └── clevomof.dll                 # Biblioteca oficial de metadados ACPI da Clevo
│
├── src/                             # Código-fonte C# (.NET Framework 4.8 / C# 5)
│   ├── App/                         # Aplicação Principal
│   │   ├── AssemblyInfo.cs          # Metadados e versão do executável
│   │   ├── Program.cs               # Ponto de entrada e controle de instância única (Mutex)
│   │   ├── MainForm.cs              # Interface gráfica Dark Theme, Tooltips e Sleep/Resume
│   │   ├── AppSettings.cs           # Persistência de configurações e boot via Task Scheduler
│   │   ├── HotkeyManager.cs         # Gerenciador de atalhos globais (Win32) e eventos ACPI
│   │   ├── HotkeySettingsForm.cs    # Interface gráfica de personalização de atalhos
│   │   └── ClevoController.cs       # Comunicação de baixo nível WMI/ACPI (SetKBLED)
│   │
│   ├── Installer/                   # Aplicativo Instalador
│   │   ├── AssemblyInfo.cs          # Metadados do instalador
│   │   ├── InstallerProgram.cs      # Ponto de entrada do instalador
│   │   └── InstallerForm.cs         # Interface gráfica com extração de recursos embutidos
│   │
│   └── Uninstaller/                 # Aplicativo Desinstalador
│       ├── AssemblyInfo.cs          # Metadados do desinstalador
│       ├── UninstallerProgram.cs    # Ponto de entrada do desinstalador
│       └── UninstallerForm.cs       # Interface gráfica de desinstalação segura
│
├── scripts/                         # Scripts de compilação e automação
│   ├── build_all.bat                # Compila o aplicativo, instalador e desinstalador
│   ├── build.bat                    # Compila apenas o aplicativo principal
│   ├── build_installer.bat          # Compila o instalador embutindo os binários
│   ├── instalar.bat                 # Script transparente de instalação aberta
│   ├── desinstalar.bat              # Script de desinstalação
│   └── registrar_driver_admin.bat   # Script para registrar os metadados ACPI no Windows
│
├── .github/                         # Modelos de Issues da comunidade para o GitHub
├── .gitignore                       # Arquivos ignorados pelo controle de versão Git
├── LICENSE                          # Licença principal do projeto (MIT)
├── THIRD_PARTY_NOTICES.md           # Avisos legais e licenças de componentes de terceiros
├── CONTRIBUTING.md                  # Guia de contribuição da comunidade
├── CHANGELOG.md                     # Histórico de alterações e controle de versão
└── README.md                        # Documentação oficial do projeto
```

---

## ⚙️ Como Recompilar do Zero

O projeto utiliza o compilador oficial da Microsoft (`csc.exe`) nativo do Windows, sem necessidade de instalar SDKs ou ferramentas adicionais:

```bat
scripts\build_all.bat
```

Os novos executáveis serão gerados automaticamente na pasta `bin\`.

---

## 🛠️ Detalhes de Baixo Nível

1. **Controlador ACPI WMI:** O teclado do notebook utiliza a interface `ACPI\PNP0C14\0` gerenciada pelo driver de kernel nativo `wmiacpi.sys`.
2. **Metadados Clevo (`clevomof.dll`):** Registrado no serviço `HKLM\SYSTEM\CurrentControlSet\Services\WmiAcpi\MofImagePath`, expondo a classe `CLEVO_GET` com o método `SetKBLED`.
3. **Protocolo de Cores:**
   - O firmware da Clevo utiliza codificação por canal: `(numBlue << 16) + green + (red << 8)`.
   - Máscaras de zona: Esquerda (`0xF0000000`), Centro (`0xF1000000`), Direita (`0xF2000000`).
   - Máscaras de brilho: `0xF4000000 | nível`.
4. **Boot Elevado:** Inicialização via tarefa agendada `schtasks /create /tn "OpenClevoRGB" /tr "\"Caminho\OpenClevoRGB.exe\" -silent" /sc onlogon /rl highest /f`.

---

## 🤖 Desenvolvimento e Créditos de IA

Este projeto foi concebido e programado em regime de **Pair Programming** com o modelo de inteligência artificial **Google Gemini (Gemini 3.8 Flash)** durante todo o ciclo de desenvolvimento:
- Investigação e identificação de hardware e barramento ACPI PNP0C14 no Windows.
- Engenharia reversa e mapeamento dos pacotes binários WMI da Clevo (`SetKBLED`).
- Arquitetura nativa em C# compilada diretamente via `csc.exe` (sem dependências de SDK).
- Design e estilização da interface gráfica em Dark Mode Moderno (Cyberpunk Glass).
- Criação dos utilitários de instalação independente e desinstalação segura.

---

## 📄 Licença e Créditos

- **Código-fonte:** Distribuído sob a licença **[MIT](LICENSE)** © 2026 Felipe Lima.
- **Componentes de Hardware e Terceiros:** Informações sobre o arquivo de metadados `clevomof.dll` e projetos de referência da comunidade estão detalhados em **[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)**.

