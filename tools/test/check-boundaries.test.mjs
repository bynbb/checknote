import childProcess from 'node:child_process';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import url from 'node:url';

const repoRoot = path.resolve(path.dirname(url.fileURLToPath(import.meta.url)), '..', '..');
const checkerPath = path.join(repoRoot, 'tools', 'check-boundaries.mjs');

const rules = {
  layers: ['domain', 'application', 'infrastructure', 'presentation', 'composition'],
  modules: ['users', 'todos'],
  allowPresentationToCompositionTokens: true,
};

const cases = [
  {
    name: 'allows application ports implemented by infrastructure',
    files: {
      'src/modules/todos/domain/todos/todo.ts': 'export interface Todo { readonly title: string; }\n',
      'src/modules/todos/application/abstractions/todo-repository.ts':
        "import { Todo } from '../../domain/todos/todo';\nexport interface TodoRepository { getAll(): Todo[]; }\n",
      'src/modules/todos/infrastructure/todos/local-storage-todo-repository.ts':
        "import { TodoRepository } from '../../application/abstractions/todo-repository';\nexport class LocalStorageTodoRepository implements TodoRepository { getAll() { return []; } }\n",
      'src/modules/todos/composition/todos/todos.tokens.ts': "export const TODO_REPOSITORY = Symbol('TODO_REPOSITORY');\n",
      'src/modules/todos/presentation/todos/todos-page.facade.ts':
        "import { TODO_REPOSITORY } from '../../composition/todos/todos.tokens';\nexport const token = TODO_REPOSITORY;\n",
      'src/app/app.component.ts':
        "import '../modules/todos/presentation/todos/todos-page.facade';\nimport '../modules/todos/composition/todos/todos.tokens';\n",
    },
    status: 0,
  },
  {
    name: 'rejects application importing infrastructure',
    files: {
      'src/modules/todos/infrastructure/todos/local-storage-todo-repository.ts': 'export class LocalStorageTodoRepository {}\n',
      'src/modules/todos/application/todos/use-case.ts':
        "import { LocalStorageTodoRepository } from '../../infrastructure/todos/local-storage-todo-repository';\nexport const repo = LocalStorageTodoRepository;\n",
    },
    status: 1,
    includes: 'application may not depend on infrastructure.',
  },
  {
    name: 'rejects source-root imports through configured src dir',
    files: {
      'src/modules/todos/infrastructure/todos/local-storage-todo-repository.ts': 'export class LocalStorageTodoRepository {}\n',
      'src/modules/todos/application/todos/use-case.ts':
        "import { LocalStorageTodoRepository } from 'src/modules/todos/infrastructure/todos/local-storage-todo-repository';\nexport const repo = LocalStorageTodoRepository;\n",
    },
    status: 1,
    includes: 'application may not depend on infrastructure.',
  },
  {
    name: 'rejects cdev namespace imports through configured src dir',
    files: {
      'src/modules/todos/infrastructure/todos/local-storage-todo-repository.ts': 'export class LocalStorageTodoRepository {}\n',
      'src/modules/todos/application/todos/use-case.ts':
        "import { LocalStorageTodoRepository } from '@cdev/modules/todos/infrastructure/todos/local-storage-todo-repository';\nexport const repo = LocalStorageTodoRepository;\n",
    },
    status: 1,
    includes: 'application may not depend on infrastructure.',
  },
  {
    name: 'rejects forbidden re-exports',
    files: {
      'src/modules/todos/infrastructure/todos/local-storage-todo-repository.ts': 'export class LocalStorageTodoRepository {}\n',
      'src/modules/todos/application/index.ts':
        "export { LocalStorageTodoRepository } from '../infrastructure/todos/local-storage-todo-repository';\n",
    },
    status: 1,
    includes: 'application may not depend on infrastructure.',
  },
  {
    name: 'rejects presentation importing infrastructure',
    files: {
      'src/modules/todos/infrastructure/todos/local-storage-todo-repository.ts': 'export class LocalStorageTodoRepository {}\n',
      'src/modules/todos/presentation/todos/todos-page.facade.ts':
        "import { LocalStorageTodoRepository } from '../../infrastructure/todos/local-storage-todo-repository';\nexport const repo = LocalStorageTodoRepository;\n",
    },
    status: 1,
    includes: 'presentation may not depend on infrastructure.',
  },
  {
    name: 'rejects domain importing Angular',
    files: {
      'src/modules/todos/domain/todos/todo.ts': "import { Injectable } from '@angular/core';\nexport const token = Injectable;\n",
    },
    status: 1,
    includes: 'domain may not depend on Angular.',
  },
  {
    name: 'rejects cross-module imports',
    files: {
      'src/modules/users/domain/users/user.ts': 'export interface User { readonly name: string; }\n',
      'src/modules/todos/application/todos/use-case.ts':
        "import { User } from '../../../users/domain/users/user';\nexport type TodoUser = User;\n",
    },
    status: 1,
    includes: 'cross-module imports between todos and users are not allowed.',
  },
  {
    name: 'rejects infrastructure importing presentation',
    files: {
      'src/modules/todos/presentation/todos/todos-page.component.ts': 'export class TodosPageComponent {}\n',
      'src/modules/todos/infrastructure/todos/local-storage-todo-repository.ts':
        "import { TodosPageComponent } from '../../presentation/todos/todos-page.component';\nexport const component = TodosPageComponent;\n",
    },
    status: 1,
    includes: 'infrastructure may not depend on presentation.',
  },
];

const failures = [];

for (const testCase of cases) {
  const tempRoot = fs.mkdtempSync(path.join(os.tmpdir(), 'checknote-boundaries-'));

  try {
    const rulesPath = path.join(tempRoot, 'tools', 'architecture-rules.json');
    writeFile(rulesPath, `${JSON.stringify(rules, null, 2)}\n`);

    for (const [relativePath, contents] of Object.entries(testCase.files)) {
      writeFile(path.join(tempRoot, relativePath), contents);
    }

    const result = childProcess.spawnSync(
      process.execPath,
      [
        checkerPath,
        '--root-dir',
        tempRoot,
        '--src-dir',
        path.join(tempRoot, 'src'),
        '--rules',
        rulesPath,
      ],
      { encoding: 'utf8' },
    );
    const output = `${result.stdout}\n${result.stderr}`;

    if (result.status !== testCase.status) {
      failures.push(`${testCase.name}: expected status ${testCase.status}, got ${result.status}.\n${output}`);
      continue;
    }

    if (testCase.includes && !output.includes(testCase.includes)) {
      failures.push(`${testCase.name}: expected output to include "${testCase.includes}".\n${output}`);
    }
  } finally {
    fs.rmSync(tempRoot, { recursive: true, force: true });
  }
}

if (failures.length > 0) {
  console.error(failures.join('\n\n'));
  process.exit(1);
}

console.log(`Architecture checker tests passed (${cases.length} cases).`);

function writeFile(file, contents) {
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, contents);
}
