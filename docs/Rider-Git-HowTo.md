# Working with Git in JetBrains Rider

This guide shows how to discard local changes and switch to the `feature/web-frontend` branch using JetBrains Rider (no command line needed).

If you prefer a quick summary, jump to:
- Discard all local changes
- Switch/checkout the `feature/web-frontend` branch
- Troubleshooting and alternatives (stash, shelve, clean, reset)

## Prerequisites
- The project is already opened in Rider.
- Git is enabled for the project (Rider detects it automatically).

## Open the Git tools in Rider
- Commit tool window: View > Tool Windows > Commit (or press Alt+0 / Cmd+0 depending on keymap).
- Git Branches popup: Click the Git branch name in the status bar (bottom right), or VCS > Git > Branches.

## Discard local changes (without command line)
Use this when you want to throw away all local modifications in the working tree.

Option A — Discard from the Commit tool window:
1) Open the Commit tool window.
2) In the Local Changes tab, review files.
3) To discard changes in specific files: right‑click the file(s) > Rollback.
4) To discard all changes in the entire working tree: select the root node > Rollback.
   - This restores tracked files to the last committed state.

Option B — Revert a file from the editor gutter:
1) Open a modified file.
2) In the left gutter (blue change markers), right‑click > Revert.

Untracked files and folders (new files):
- The Rollback/Revert actions affect tracked files. To delete untracked files:
  - Right‑click untracked files in the Commit tool window > Delete, or
  - Use Git > Repository > Clean… (safe UI for git clean). Choose what to remove and confirm.

Keep changes for later (optional):
- Stash: Git > Stash Changes… (later apply via Git > Unstash Changes…)
- Shelve: VCS > Shelf > Shelve Changes (IDE-managed; apply later via Unshelve)

## Switching to the feature/web-frontend branch

If the branch already exists locally:
1) Click the current branch name in the status bar (bottom-right) to open the Branches popup.
2) Under Local Branches, select feature/web-frontend > Checkout.

If the branch exists on the remote only:
1) Open the Branches popup.
2) Click Remote Branches > origin/feature/web-frontend.
3) Choose Checkout (or Checkout as New Local Branch) and keep the default branch name.

If you don’t see the branch yet:
1) VCS > Git > Fetch (or use the Fetch button in the Branches popup).
2) Repeat the steps to checkout origin/feature/web-frontend.

## What if Rider says checkout is blocked by local changes?
You have several safe options. Pick one:
- Smart checkout with automatic stash: In the checkout dialog, enable the option to stash local changes and reapply after checkout.
- Manually stash: Git > Stash Changes…; then retry the checkout. If needed, Unstash after switching.
- Shelve (IDE alternative): VCS > Shelf > Shelve Changes; then checkout. Unshelve later if desired.
- Discard changes: Use Rollback (see above) if you are sure the changes aren’t needed.

## Resetting branch to the remote (advanced, no terminal)
If you want your current branch to exactly match the remote (discarding local commits):
1) VCS > Git > Log.
2) In the Log, find the commit you want to reset to (e.g., origin/your-branch tip).
3) Right‑click the commit > Reset Current Branch to Here…
4) Choose Hard (discard working tree and index), Mixed, or Soft as needed. Hard = discards everything local.

## Cleaning untracked files (UI for git clean)
- Git > Repository > Clean…
- Choose what to remove (untracked files, ignored files) and confirm.

## Troubleshooting
- Detached HEAD: If you checked out a specific commit, Rider shows Detached HEAD. Use Branches popup > New Branch from ‘HEAD’ to create a branch, or checkout an existing branch.
- Upstream tracking: After creating a local branch from a remote, Rider configures the upstream automatically. Verify in Branches popup > Configure Branch.…
- Authentication: If fetching doesn’t show the branch, confirm remote URL and credentials in Preferences > Version Control > Git > Remotes.

## Quick checklist to switch to feature/web-frontend now
1) Save or discard local changes:
   - Stash/Shelve if you might need them later, or Rollback/Clean to discard.
2) Fetch remotes:
   - VCS > Git > Fetch
3) Checkout the branch:
   - Branches popup > origin/feature/web-frontend > Checkout
4) Verify you’re on the branch:
   - Status bar shows feature/web-frontend; VCS > Git > Log highlights the branch head.
