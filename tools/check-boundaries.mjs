import fs from 'node:fs';
import path from 'node:path';
import url from 'node:url';

const defaultRootDir = path.resolve(path.dirname(url.fileURLToPath(import.meta.url)), '..');
const options = parseOptions(process.argv.slice(2));
const rootDir = options.rootDir ? path.resolve(options.rootDir) : defaultRootDir;
const srcDir = options.srcDir ? path.resolve(options.srcDir) : path.join(rootDir, 'src');
const rulesPath = options.rulesPath ? path.resolve(options.rulesPath) : path.join(rootDir, 'tools', 'architecture-rules.json');
const rules = JSON.parse(fs.readFileSync(rulesPath, 'utf8'));
const tsFiles = listFiles(srcDir).filter((file) => file.endsWith('.ts'));
const violations = [];

for (const file of tsFiles) {
  const source = fs.readFileSync(file, 'utf8');
  const imports = readImports(source);
  const from = classify(file);

  for (const specifier of imports) {
    const target = resolveImport(file, specifier);

    if (specifier.startsWith('@angular/material')) {
      addViolation(file, '@angular/material', 'Checknote uses native CSS plus Angular CDK behavior primitives, not Angular Material.');
      continue;
    }

    if (specifier.startsWith('@angular/')) {
      validateAngularImport(file, from);
      continue;
    }

    if (!target || !isInside(target, srcDir)) {
      continue;
    }

    validateInternalImport(file, target, from, classify(target));
  }
}

if (violations.length > 0) {
  console.error('Architecture violation:');
  for (const violation of violations) {
    console.error(`${toRepoPath(violation.file)} imported ${toRepoPath(violation.imported)}.`);
    console.error(`${violation.reason}\n`);
  }
  process.exit(1);
}

const stamp = options.stamp;
if (stamp) {
  fs.writeFileSync(path.resolve(rootDir, stamp), 'ok\n');
}

function parseOptions(args) {
  const parsed = {
    rootDir: null,
    srcDir: null,
    rulesPath: null,
    stamp: null,
  };

  for (let index = 0; index < args.length; index += 1) {
    const arg = args[index];
    const next = args[index + 1];

    if (arg === '--root-dir') {
      parsed.rootDir = requireValue(arg, next);
      index += 1;
      continue;
    }

    if (arg === '--src-dir') {
      parsed.srcDir = requireValue(arg, next);
      index += 1;
      continue;
    }

    if (arg === '--rules') {
      parsed.rulesPath = requireValue(arg, next);
      index += 1;
      continue;
    }

    if (arg === '--stamp') {
      parsed.stamp = requireValue(arg, next);
      index += 1;
      continue;
    }

    if (arg.startsWith('--')) {
      throw new Error(`Unknown option: ${arg}`);
    }

    parsed.stamp = arg;
  }

  return parsed;
}

function requireValue(option, value) {
  if (!value || value.startsWith('--')) {
    throw new Error(`${option} requires a value.`);
  }

  return value;
}

function listFiles(directory) {
  const entries = fs.readdirSync(directory, { withFileTypes: true });
  return entries.flatMap((entry) => {
    const fullPath = path.join(directory, entry.name);
    return entry.isDirectory() ? listFiles(fullPath) : [fullPath];
  });
}

function readImports(source) {
  const imports = [];
  const importPattern = /import\s+(?:type\s+)?(?:[^'"]*?\s+from\s+)?['"]([^'"]+)['"]/g;
  const exportPattern = /export\s+(?:type\s+)?(?:[^'"]*?\s+from\s+)?['"]([^'"]+)['"]/g;
  const dynamicPattern = /import\(\s*['"]([^'"]+)['"]\s*\)/g;
  let match = importPattern.exec(source);

  while (match) {
    imports.push(match[1]);
    match = importPattern.exec(source);
  }

  match = exportPattern.exec(source);
  while (match) {
    imports.push(match[1]);
    match = exportPattern.exec(source);
  }

  match = dynamicPattern.exec(source);
  while (match) {
    imports.push(match[1]);
    match = dynamicPattern.exec(source);
  }

  return imports;
}

function resolveImport(fromFile, specifier) {
  if (!specifier.startsWith('.')) {
    if (specifier.startsWith('@cdev/')) {
      return resolveExisting(path.join(srcDir, specifier.slice('@cdev/'.length)));
    }

    return specifier.startsWith('src/') ? resolveExisting(path.join(srcDir, specifier.slice('src/'.length))) : null;
  }

  return resolveExisting(path.resolve(path.dirname(fromFile), specifier));
}

function resolveExisting(basePath) {
  const candidates = [
    `${basePath}.ts`,
    path.join(basePath, 'index.ts'),
    basePath,
  ];

  return candidates.find((candidate) => fs.existsSync(candidate)) ?? null;
}

function isInside(file, directory) {
  const relative = path.relative(directory, file);
  return relative === '' || (!!relative && !relative.startsWith('..') && !path.isAbsolute(relative));
}

function classify(file) {
  const relative = toSourcePath(file);
  const parts = relative.split('/');

  if (parts[0] === 'app' || parts[0] === 'main.ts') {
    return { area: 'app' };
  }

  if (parts[0] === 'common' && rules.layers.includes(parts[1])) {
    return { area: 'common', layer: parts[1] };
  }

  if (parts[0] === 'modules' && parts.length > 2 && rules.modules.includes(parts[1]) && rules.layers.includes(parts[2])) {
    return { area: 'module', module: parts[1], layer: parts[2] };
  }

  return { area: 'other' };
}

function validateAngularImport(file, from) {
  if (from.layer === 'domain' || from.layer === 'application' || from.layer === 'infrastructure') {
    addViolation(file, '@angular/*', `${from.layer} may not depend on Angular.`);
  }
}

function validateInternalImport(file, target, from, to) {
  if (from.area === 'app') {
    if (to.area === 'module' && to.layer !== 'composition' && to.layer !== 'presentation') {
      addViolation(file, target, 'app may import module composition exports and presentation entry points only.');
    }

    if (to.area === 'common' && !['composition', 'presentation'].includes(to.layer)) {
      addViolation(file, target, 'app may import common composition exports and presentation entry points only.');
    }

    return;
  }

  if (from.area === 'common') {
    validateCommonImport(file, target, from, to);
    return;
  }

  if (from.area !== 'module') {
    return;
  }

  if (to.area === 'module' && from.module !== to.module) {
    addViolation(file, target, `cross-module imports between ${from.module} and ${to.module} are not allowed.`);
    return;
  }

  if (to.area === 'common') {
    validateCommonImport(file, target, from, to);
    return;
  }

  if (to.area === 'module') {
    validateModuleLayerImport(file, target, from, to);
  }
}

function validateCommonImport(file, target, from, to) {
  if (to.area !== 'common') {
    return;
  }

  if (from.layer === 'domain' && to.layer !== 'domain') {
    addViolation(file, target, 'domain may depend only on common/domain.');
  }

  if (from.layer === 'application' && !['domain', 'application'].includes(to.layer)) {
    addViolation(file, target, 'application may depend only on common/domain and common/application.');
  }

  if (from.layer === 'presentation' && to.layer === 'infrastructure') {
    addViolation(file, target, 'presentation may not depend on common/infrastructure.');
  }
}

function validateModuleLayerImport(file, target, from, to) {
  if (from.layer === 'domain' && to.layer !== 'domain') {
    addViolation(file, target, 'domain may not depend on application, infrastructure, presentation, or composition.');
  }

  if (from.layer === 'application' && ['infrastructure', 'presentation', 'composition'].includes(to.layer)) {
    addViolation(file, target, `application may not depend on ${to.layer}.`);
  }

  if (from.layer === 'infrastructure' && ['presentation', 'composition'].includes(to.layer)) {
    addViolation(file, target, `infrastructure may not depend on ${to.layer}.`);
  }

  if (from.layer === 'presentation' && to.layer === 'infrastructure') {
    addViolation(file, target, 'presentation may not depend on infrastructure.');
  }

  if (
    from.layer === 'presentation' &&
    to.layer === 'composition' &&
    (!rules.allowPresentationToCompositionTokens || !target.endsWith('.tokens.ts'))
  ) {
    addViolation(file, target, 'presentation may import only composition token files.');
  }
}

function addViolation(file, imported, reason) {
  violations.push({ file, imported, reason });
}

function toRepoPath(file) {
  return path.relative(rootDir, file).replaceAll(path.sep, '/');
}

function toSourcePath(file) {
  return path.relative(srcDir, file).replaceAll(path.sep, '/');
}
