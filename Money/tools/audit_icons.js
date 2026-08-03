const fs = require("fs");
const path = require("path");

const root = path.resolve(__dirname, "..");
const sourceFiles = [];

function collect(directory) {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    if (entry.isDirectory() && ["bin", "obj", ".git"].includes(entry.name)) continue;
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) collect(fullPath);
    else if (/\.(?:xaml|cs)$/i.test(entry.name)) sourceFiles.push(fullPath);
  }
}

collect(root);

const xamlFiles = sourceFiles.filter(file => file.endsWith(".xaml"));
const legacyReferences = [];
const missingMaterialNamespace = [];
const missingSourceNamespace = [];
const inconsistentBack = [];
const themeDependentClickableIcons = [];
const imageButtonsWithoutExplicitSource = [];
const mauiIconsWithoutExplicitColor = [];
const textIcons = [];
const emojiPattern = /[\u2600-\u27BF]|\p{Extended_Pictographic}/u;

for (const file of sourceFiles) {
  const content = fs.readFileSync(file, "utf8");
  const relative = path.relative(root, file);

  if (/\.(?:svg|png)/i.test(content) || /FileImageSource|new\s+Image\b/i.test(content)) {
    legacyReferences.push(relative);
  }
  if (emojiPattern.test(content)) textIcons.push(relative);

  if (!file.endsWith(".xaml")) continue;
  if (/<Image\b/i.test(content)) legacyReferences.push(relative);
  if (/mi:/.test(content) && !/xmlns:mi="http:\/\/www\.aathifmahir\.com\/dotnet\/2022\/maui\/icons"/.test(content)) {
    missingMaterialNamespace.push(relative);
  }
  if (/icons:/.test(content) && !/xmlns:icons="clr-namespace:Money"/.test(content)) {
    missingSourceNamespace.push(relative);
  }

  if (/(?:Source|ImageSource|IconImageSource)="\{mi:Material\b/.test(content)) {
    themeDependentClickableIcons.push(relative);
  }

  for (const match of content.matchAll(/<mi:MauiIcon\b.*?\/>/gs)) {
    if (!/IconColor=/.test(match[0])) mauiIconsWithoutExplicitColor.push(relative);
  }

  for (const match of content.matchAll(/<(?:Button|ImageButton)\b.*?\/>/gs)) {
    const tag = match[0];
    if (/^<ImageButton\b/.test(tag) && !/Source="\{icons:MaterialIconSource\b/.test(tag)) {
      imageButtonsWithoutExplicitSource.push(relative);
    }
  }

  for (const match of content.matchAll(/<ImageButton\b[^>]*Clicked="On(?:Back|Close|Cancel)Clicked"[^>]*\/?\s*>/gs)) {
    const tag = match[0];
    if (!/Source="\{icons:MaterialIconSource\b[^}]*ColorResource=TextPrimary/.test(tag) ||
        !/BackgroundColor="\{StaticResource CardBackground\}"/.test(tag) ||
        !/WidthRequest="44"/.test(tag) || !/HeightRequest="44"/.test(tag)) {
      inconsistentBack.push(relative);
    }
  }
}

const unique = values => [...new Set(values)].sort();
const legacy = unique(legacyReferences);
const materialNamespaces = unique(missingMaterialNamespace);
const sourceNamespaces = unique(missingSourceNamespace);
const backButtons = unique(inconsistentBack);
const themeDependent = unique(themeDependentClickableIcons);
const implicitImageButtons = unique(imageButtonsWithoutExplicitSource);
const colorlessMauiIcons = unique(mauiIconsWithoutExplicitColor);
const textIconFiles = unique(textIcons);

console.log(`XAML verificados: ${xamlFiles.length}`);
console.log(`Referencias legadas: ${legacy.length}`);
console.log(`Namespaces Material ausentes: ${materialNamespaces.length}`);
console.log(`Namespaces de fonte explicita ausentes: ${sourceNamespaces.length}`);
console.log(`Botoes Voltar fora do padrao: ${backButtons.length}`);
console.log(`Icones clicaveis dependentes do tema: ${themeDependent.length}`);
console.log(`ImageButtons sem fonte explicita: ${implicitImageButtons.length}`);
console.log(`MauiIcons sem cor explicita: ${colorlessMauiIcons.length}`);
console.log(`Icones Unicode/emojis: ${textIconFiles.length}`);
if (legacy.length) console.log(legacy.join("\n"));
if (materialNamespaces.length) console.log(materialNamespaces.join("\n"));
if (sourceNamespaces.length) console.log(sourceNamespaces.join("\n"));
if (backButtons.length) console.log(backButtons.join("\n"));
if (themeDependent.length) console.log(themeDependent.join("\n"));
if (implicitImageButtons.length) console.log(implicitImageButtons.join("\n"));
if (colorlessMauiIcons.length) console.log(colorlessMauiIcons.join("\n"));
if (textIconFiles.length) console.log(textIconFiles.join("\n"));
process.exit(legacy.length || materialNamespaces.length || sourceNamespaces.length || backButtons.length ||
  themeDependent.length || implicitImageButtons.length || colorlessMauiIcons.length || textIconFiles.length ? 1 : 0);
