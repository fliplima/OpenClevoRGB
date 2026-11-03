# Changelog

Todas as alterações notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Versionamento Semântico](https://semver.org/lang/pt-BR/).

---

## [1.4.0] - 2026-11-03

### Adicionado
- **Suporte Completo a Atalhos Globais e Teclas Fn do Notebook:**
  - **Mapeamento das Teclas Físicas Clevo:** Mapeamento padrão dos ícones gravados nas teclas do teclado numérico:
    - `Fn + / (NumPad)` / `Ctrl + Num /`: **Trocar Cor** (ciclo dinâmico entre as 12 cores elegantes da paleta).
    - `Fn + * (NumPad)` / `Ctrl + Num *`: **Ligar / Desligar Luz** (toggle on/off com restauração do nível de brilho).
    - `Fn + + (NumPad)` / `Ctrl + Num +`: **Aumentar Brilho** (incremento de nível 0% -> 25% -> 50% -> 75% -> 100%).
    - `Fn + - (NumPad)` / `Ctrl + Num -`: **Reduzir Brilho** (decremento de nível 100% -> 75% -> 50% -> 25% -> 0%).
  - **Captura Dual (Hardware ACPI + Windows Global Hotkeys):**
    - `ClevoEventListener`: Escuta eventos de hardware ACPI em background (`CLEVO_EVENT` em `root\wmi`) emitidos pela BIOS/EC ao pressionar `Fn + NumPad`.
    - `HotkeyManager`: Registra atalhos globais de sistema no Windows (`RegisterHotKey`) que funcionam mesmo enquanto você joga ou utiliza qualquer outro software, com o utilitário minimizado na bandeja.
  - **Interface Completa de Personalização de Atalhos (`HotkeySettingsForm`):**
    - Nova tela dedicada em Cyberpunk Dark Theme aberta pelo botão `⌨️ Personalizar...`.
    - Permite configurar livremente os modificadores (`Ctrl`, `Alt`, `Shift`, `Win` ou nenhum) e qualquer tecla do teclado (NumPad, Teclas F1-F12, Setas, Navegação, Letras e Números).
    - Botão *"Restaurar Padrões"* para voltar rapidamente às teclas padrão do teclado numérico.
  - **Feedback Visual na Bandeja do Sistema:**
    - Exibição de avisos sutis (Balloon Tips) ao alterar brilho, cor ou ligar/desligar luzes com o aplicativo em segundo plano.

---

## [1.3.0] - 2026-10-22

### Adicionado
- **Renomeação Oficial para OpenClevoRGB:**
  - O projeto foi renomeado de *Avell RGB Keyboard Master* para **`OpenClevoRGB`**, refletindo a compatibilidade universal com laptops baseados em chassis Clevo (Avell, Sager, Hasee, Schenker, etc.).
- **Restauração Automática após Suspensão (Sleep / Resume):**
  - Implementado ouvinte do evento nativo `SystemEvents.PowerModeChanged`. O aplicativo restaura silenciosamente as cores e o brilho configurados 1,5s após o notebook acordar da suspensão ou hibernação, resolvendo o bug crônico da BIOS Clevo que reseta as luzes para azul.
- **Ícone Oficial do Windows (`resources/app.ico`):**
  - Geração e vinculação de ícone nativo multiplataforma (16px, 32px, 48px) em todos os executáveis compilados (`OpenClevoRGB.exe`, `Instalador_OpenClevoRGB.exe`, `Desinstalador.exe`), exibindo o teclado RGB no Windows Explorer, Área de Trabalho e Barra de Tarefas.
- **Metadados de Informações de Versão (`AssemblyInfo.cs`):**
  - Metadados formais de produto, autor, versão e direitos autorais embutidos em todos os binários para compliance com o Windows SmartScreen.
- **Desinstalador Gráfico Dedicado (`Desinstalador.exe`):**
  - Aplicativo visual com janela Dark Theme de confirmação.
  - Remove processos em execução, atalhos, tarefa agendada no boot e arquivos com auto-exclusão segura.
  - Opção para o usuário escolher se deseja manter ou apagar as preferências de cores (`%AppData%`).
  - Criação automática do atalho *"Desinstalar OpenClevoRGB"* no Menu Iniciar.
  - Integração nativa com o Windows (*Painel de Controle* e *Configurações > Aplicativos Instalados*).
- **Instalador 100% Autossuficiente e Portátil:**
  - O executável `bin\Instalador_OpenClevoRGB.exe` agora embute `OpenClevoRGB.exe`, `clevomof.dll` e `Desinstalador.exe`.
- **Script de Instalação Aberto/Transparente (`instalar.bat`):**
  - Script em lote puro (`scripts\instalar.bat` e atalho na raiz) que permite a usuários inspecionar abertamente todos os comandos de instalação executados no sistema.
- **Padrões de Comunidade do GitHub:**
  - Adicionado `CONTRIBUTING.md`.
  - Adicionados templates para Issues de bug e sugestões em `.github/ISSUE_TEMPLATE/`.
- **Script de Desinstalação (`scripts\desinstalar.bat`):**
  - Remoção completa de arquivos, atalhos, tarefa agendada e entradas de registro.
- **Preparação para Repositório Público no GitHub:**
  - Arquivo `.gitignore` configurado.
  - Licença oficial open-source `LICENSE` (MIT).
  - Documento de avisos legais e direitos de terceiros `THIRD_PARTY_NOTICES.md` (para a biblioteca `clevomof.dll`).
  - Badges e instruções completas no `README.md`.

---

## [1.2.0] - 2026-10-22

### Adicionado
- **Instalador Oficial Independente (`Instalador_AvellRGB.exe`):**
  - Assistente gráfico completo no mesmo visual Dark Theme Cyberpunk.
  - Suporte para o usuário escolher o diretório de destino da instalação (padrão em `C:\Program Files\Avell RGB Keyboard Master`).
  - Opções selecionáveis para criação de atalhos na Área de Trabalho (Desktop) e no Menu Iniciar.
  - Registro de metadados de driver ACPI embutido no processo de instalação.
  - Registro no Painel de Controle e Configurações do Windows (*Adicionar ou Remover Programas*) com script de desinstalação automática (`Desinstalar.bat`).
- **Automação de Build:**
  - Script `scripts/build_all.bat` para compilar o aplicativo principal e o instalador em sequência.
  - Script `scripts/build_installer.bat` com embutimento de binários via recursos PE.
- **Controle de Versão:**
  - Criação do arquivo de documentação `CHANGELOG.md`.

### Modificado
- **Inicialização Automática com o Windows:**
  - Substituição da chave de registro `Run` por tarefa no **Agendador de Tarefas do Windows (`Task Scheduler`)** com privilégios elevados (`/rl highest`).
  - Corrige o bloqueio de boot do UAC do Windows para aplicativos que exigem permissões de Administrador, permitindo que o utilitário inicialize silenciosamente na bandeja do sistema.
- **Reorganização Estrutural do Repositório:**
  - Estruturação modular limpa em pastas: `bin/`, `resources/`, `scripts/`, `src/App/` e `src/Installer/`.

---

## [1.1.0] - 2026-10-21

### Adicionado
- **Dicas Rápidas no Hover (Tooltips):**
  - Sistema de ToolTips personalizados em Dark Mode para todos os botões, checkboxes, zonas e seletores.
- **Atalhos Rápidos de Luminosidade:**
  - 5 botões de clique rápido: `Off (0%)`, `25%`, `50%`, `75%` e `100% (Máx)`.
- **Renderização Dark Customizada para ComboBox:**
  - Implementação de `DrawMode.OwnerDrawFixed` para eliminar o fundo cinza clássico e aplicar a estética escura moderna.

### Modificado
- **Refatoração de Layout e Ergonomia:**
  - Expansão da largura do painel principal para 880px, eliminando corte de textos em telas de alta densidade (DPI Scaling).
  - Simplificação dos textos dos botões das zonas (`ESQUERDA`, `CENTRO`, `DIREITA`), movendo as informações detalhadas de teclas para a dica do cursor.
  - Botão de seletor avançado ajustado para `🎨 Mais Cores...` em linha única.
  - Ajuste nas margens e contêiner do badge de status (`Driver WMI Ativo`).

### Corrigido
- **Acesso Negado no WMI ACPI:**
  - Adicionada a diretiva `requireAdministrator` no manifesto da aplicação (`app.manifest`), resolvendo o bloqueio de segurança do Windows ao consultar métodos da classe `CLEVO_GET`.
  - Detecção dinâmica de instâncias de hardware WMI para maior compatibilidade.

---

## [1.0.0] - 2026-10-21

### Adicionado
- **Lançamento Inicial do Avell RGB Keyboard Master:**
  - Suporte completo ao controlador ACPI Clevo N850HP6 / N870HP6 (notebooks Avell).
  - Controle independente para as 3 zonas RGB do teclado (Esquerda, Centro e Direita).
  - Modo unificado para sincronização de todas as zonas.
  - Paleta com 12 cores rápidas e seletor completo de 16,7 milhões de cores via `ColorDialog`.
  - Slider contínuo de intensidade de brilho (0 a 100%) e botão de ligar/desligar LEDs.
  - Suporte aos 6 efeitos pré-programados na BIOS (Breathe, Cycle, Wave, Dance, Flash e Random).
  - Integração com a Bandeja do Sistema (System Tray / Relógio) com menu de contexto rápido.
  - Persistência local de preferências em XML (`%AppData%\AvellKeyboardRGB\settings.xml`).
  - Módulo de baixo nível para comunicação WMI ACPI com o driver `wmiacpi.sys`.
  - Empacotamento com o compilador C# nativo (`csc.exe`) gerando executável ultraleve de ~39 KB.

