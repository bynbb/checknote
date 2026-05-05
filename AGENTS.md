# Checknote Agent Rules

## Instruction Files

The tracked agent instruction files for this repository are:

- `AGENTS.md`

Any additional tracked `AGENTS.md` file must be intentionally added to this list in the same change.

## After Push

After pushing code, check the GitHub Actions CI/CD run for the pushed commit before reporting the work as done.

Report the result to the user:

- If the run passes, say that CI/CD passed.
- If the run fails, report the failing workflow/job, the relevant error, and the most likely failure area.
- If the run cannot be checked, say that explicitly and explain why.

Do not treat a pushed change as complete until CI/CD has either passed, failed, or been explicitly reported as unavailable.
