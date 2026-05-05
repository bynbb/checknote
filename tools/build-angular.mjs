import { createRequire } from 'node:module';
import path from 'node:path';

const require = createRequire(import.meta.url);
const { Architect } = require('@angular-devkit/architect');
const { WorkspaceNodeModulesArchitectHost } = require('@angular-devkit/architect/node');
const { logging, schema, workspaces } = require('@angular-devkit/core');
const { NodeJsSyncHost } = require('@angular-devkit/core/node');

const options = parseArgs(process.argv.slice(2));
const workspaceRoot = path.resolve(options.workspace);
const configFilePath = path.join(workspaceRoot, 'angular.json');
const [project, target, configuration] = options.target.split(':');

if (!project || !target) {
  throw new Error(`Expected --target to use project:target[:configuration], got '${options.target}'.`);
}

const registry = new schema.CoreSchemaRegistry();
registry.addPostTransform(schema.transforms.addUndefinedDefaults);

const { workspace } = await workspaces.readWorkspace(
  configFilePath,
  workspaces.createWorkspaceHost(new NodeJsSyncHost()),
);

const architectHost = new WorkspaceNodeModulesArchitectHost(workspace, workspaceRoot);
const architect = new Architect(architectHost, registry);
const logger = new logging.Logger('checknote-build');

logger.subscribe((entry) => {
  if (!entry.message) {
    return;
  }

  if (entry.level === 'error' || entry.level === 'fatal') {
    console.error(entry.message);
    return;
  }

  if (entry.level === 'warn') {
    console.warn(entry.message);
    return;
  }

  console.log(entry.message);
});

const run = await architect.scheduleTarget(
  { project, target, configuration },
  {
    outputPath: path.resolve(options.output),
    progress: false,
  },
  { logger },
);

try {
  const result = await run.lastOutput;

  if (!result.success) {
    process.exitCode = 1;
  }
} finally {
  await run.stop();
}

function parseArgs(args) {
  const parsed = {
    output: null,
    target: 'checknote:build:production',
    workspace: 'cdev',
  };

  for (let index = 0; index < args.length; index += 1) {
    const arg = args[index];
    const next = args[index + 1];

    if (arg === '--output') {
      parsed.output = requireValue(arg, next);
      index += 1;
      continue;
    }

    if (arg === '--target') {
      parsed.target = requireValue(arg, next);
      index += 1;
      continue;
    }

    if (arg === '--workspace') {
      parsed.workspace = requireValue(arg, next);
      index += 1;
      continue;
    }

    throw new Error(`Unknown argument: ${arg}`);
  }

  if (!parsed.output) {
    throw new Error('--output is required.');
  }

  return parsed;
}

function requireValue(option, value) {
  if (!value || value.startsWith('--')) {
    throw new Error(`${option} requires a value.`);
  }

  return value;
}
