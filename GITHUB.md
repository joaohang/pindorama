# Versionamento com Git + GitHub

## Resposta curta

**Sim**, a Unity funciona com GitHub — mas o GitHub não é “nativo” dentro da Unity.
A Unity não tem um botão “Publicar no GitHub”; você versiona o projeto com **Git**
(CLI, GitHub Desktop, Sourcetree ou Rider/VS) e usa o GitHub como remoto.

O que já está pronto neste projeto:

- repositório Git local inicializado na branch `main`;
- primeiro commit feito;
- `.gitignore` de Unity (Library/Temp/Logs/builds fora do repositório);
- `.gitattributes` para texto/binário;
- `Packages/manifest.json` e `packages-lock.json` versionados;
- todos os `.meta` versionados.

## 1. Criar o repositório no GitHub

1. Abra <https://github.com/new>.
2. Nome: `pindorama`.
3. Escolha **Private** (recomendado no começo).
4. **Não** marque “Add a README”, “Add .gitignore” nem “Choose a license”
   (o projeto já tem esses arquivos).
5. Clique em **Create repository**.

## 2. Conectar e enviar

No terminal, dentro da pasta `pindorama`:

```powershell
git remote add origin https://github.com/SEU_USUARIO/pindorama.git
git push -u origin main
```

Se pedir login, use uma das opções:

- **GitHub Desktop** (mais simples): abra o app, faça login e adicione a pasta
  `pindorama` como repositório local; ele configura a autenticação.
- **Personal Access Token (PAT)**: GitHub → Settings → Developer settings →
  Personal access tokens → gere um token com escopo `repo`; use como senha.
- **SSH**: configure uma chave e troque a URL para
  `git@github.com:SEU_USUARIO/pindorama.git`.

## 3. Configurações importantes na Unity

Em **Edit → Project Settings → Editor**:

- **Asset Serialization:** `Force Text` (importante para diff/merge).
- **Version Control:** `Visible Meta Files` (quando disponível).
- **Nunca** versione `Library/`, `Temp/`, `Logs/` ou builds.

Os arquivos `.meta` **devem** ser commitados; sem eles a Unity recria GUIDs e
quebra referências entre cenas/prefabs.

## 4. Fluxo do dia a dia

```powershell
git status                 # ver o que mudou
git add .
git commit -m "feat: ajusta salto de gancho"
git push
```

Antes de começar a trabalhar:

```powershell
git pull
```

Para uma feature nova:

```powershell
git switch -c feat/novo-movimento
# trabalhe, commit, push
git switch main
git merge feat/novo-movimento
```

## 5. Assets grandes e Git LFS

O Git LFS já está instalado nesta máquina. Para texturas/áudio/modelos grandes,
descomente os padrões no `.gitattributes`:

```gitattributes
*.png filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.fbx filter=lfs diff=lfs merge=lfs -text
```

Depois:

```powershell
git lfs install
git add --renormalize .
git commit -m "chore: ativa Git LFS para assets"
```

## 6. Merge de cenas e prefabs

Cenas e prefabs são YAML. Para reduzir conflitos:

- prefira **Prefab Mode** e evite editar a mesma cena em duas branches;
- faça commits pequenos e frequentes;
- opcionalmente configure o **UnityYAMLMerge (SmartMerge)** como merge tool
  (`Editor/Data/Tools/UnityYAMLMerge.exe`) no Git.

## 7. CI/CD (opcional)

Para builds automáticos no GitHub Actions, o projeto `game-ci/unity-builder`
funciona bem, mas exige configurar a licença da Unity como secret
(`UNITY_LICENSE`). Para o protótipo, o fluxo manual já resolve.

## 8. Atenção: OneDrive

Este projeto está dentro do OneDrive. Isso pode causar **locks e conflitos**
enquanto a Unity está aberta (já vimos erros de escrita por isso). O ideal é
mover o projeto para uma pasta local, por exemplo:

```text
C:\Dev\Pindorama
```

e usar o GitHub como backup/histórico. Se preferir manter no OneDrive, pause a
sincronização enquanto a Unity estiver aberta.
