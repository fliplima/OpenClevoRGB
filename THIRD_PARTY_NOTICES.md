# Avisos de Terceiros e Licenças de Hardware (Third-Party Notices)

Este projeto, **OpenClevoRGB**, é distribuído sob a licença [MIT](LICENSE).

Para permitir a comunicação e interoperabilidade direta com o hardware do teclado retroiluminado, este repositório inclui ou referencia componentes de terceiros conforme detalhado abaixo.

---

## 1. CLEVO MOF Metadata Library (`clevomof.dll`)

- **Componente:** `clevomof.dll` (disponível em `resources/clevomof.dll` e `bin/clevomof.dll`)
- **Proprietário do Copyright:** © CLEVO Co. / OEM Hardware Manufacturer
- **Propósito no Projeto:**  
  Este arquivo binário contém a definição compilada em formato MOF (Managed Object Format) da interface WMI ACPI da placa-mãe (`CLEVO_GET`). Ele é registrado no subsistema do driver `wmiacpi.sys` da Microsoft para expor o método de controle de hardware `SetKBLED`.
- **Termo de Uso e Interoperabilidade:**  
  A inclusão desta biblioteca destina-se estritamente à **interoperabilidade funcional de hardware** (hardware interoperability), permitindo aos proprietários de laptops Avell / Clevo gerenciar as luzes do seu teclado na ausência do utilitário oficial descontinuado da fabricante.
- **Aviso:**  
  Todos os direitos de marca e propriedade intelectual sobre a definição de firmware e esquemas WMI pertencem à CLEVO Co. e suas marcas associadas.

---

## 2. Reconhecimento e Créditos Comunitários

O desenvolvimento dos comandos de baixo nível e a engenharia reversa do protocolo WMI foram possíveis graças a pesquisas e ferramentas abertas da comunidade Clevo/Linux:

- **[rzrwolf / LED-manager-for-Clevo](https://github.com/rzrwolf/LED-manager-for-Clevo)**  
  *Referência de pesquisa para os códigos hexadecimais de zonas e níveis de brilho no Windows.*
- **[DeviceIoControl / CppKeyboardColour](https://github.com/DeviceIoControl/CppKeyboardColour)**  
  *Referência de mapeamento das chamadas de controle de hardware em C++.*
- **[TUXEDO Computers / tuxedo-drivers (clevo-wmi)](https://github.com/tuxedocomputers/tuxedo-drivers)**  
  *Referência do driver de kernel Linux para chassis Clevo.*

---

## 3. Microsoft Windows & .NET Framework

- **Componentes:** Microsoft .NET Framework 4.8 / Windows Forms / WMI (`System.Management`)
- **Copyright:** © Microsoft Corporation
- **Termos:**  
  As compilações utilizam as bibliotecas padrão do sistema operacional Windows, sujeitas aos Termos de Licença para Software Microsoft (EULA).

