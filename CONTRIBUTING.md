# Como Contribuir com o OpenClevoRGB

Agradecemos o seu interesse em contribuir com este projeto! Toda ajuda — seja relatando bugs, sugerindo suporte a novos modelos de notebook ou enviando código — é muito bem-vinda.

---

## 🛠️ Como Configurar o Ambiente de Desenvolvimento

Este projeto foi construído para ser **100% autossuficiente no Windows**, sem exigir instalações pesadas de IDEs ou SDKs externos:

1. Clone o repositório:
   ```bash
   git clone https://github.com/SEU_USUARIO/OpenClevoRGB.git
   cd OpenClevoRGB
   ```
2. Para compilar todos os binários (Aplicativo + Instalador + Desinstalador), basta executar:
   ```bat
   scripts\build_all.bat
   ```
   *O script utiliza o compilador oficial C# nativo do Windows (`csc.exe`).*

---

## 💡 Diretrizes para Envio de Código (Pull Requests)

1. Crie uma branch para a sua alteração:
   ```bash
   git checkout -b feature/minha-melhoria
   ```
2. Mantenha a compatibilidade:
   - O projeto é escrito em **C# 5 (.NET Framework 4.8)** para garantir que qualquer usuário Windows consiga compilar diretamente com o `csc.exe` nativo sem precisar instalar ferramentas extras.
3. Teste em hardware real:
   - Se estiver adicionando novos códigos WMI ou suporte a outros modelos de chassi (ex: Tongfang, outros modelos Clevo), mencione no PR o modelo em que foi testado.
4. Envie o Pull Request com uma descrição clara do que foi alterado.

---

## 🐛 Relatando Problemas (Issues)

- Verifique se já não existe uma Issue aberta sobre o mesmo assunto.
- Ao abrir uma nova Issue, utilize os modelos disponibilizados informando o modelo exato do seu notebook e a versão do Windows.

