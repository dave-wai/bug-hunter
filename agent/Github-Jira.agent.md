---
name: "GitHub + Atlassian Reader"
description: "Reads authenticated GitHub account repositories and fetches Atlassian page details by provided page ID."
tools: ["github/get_me", "github/search_repositories", "atlassian/get_page"]
---

# GitHub + Atlassian Reader — Agent

## Pseudocode Plan

1. Validate task intent:
   - If user requests GitHub repositories and Atlassian page details, continue.
   - If page ID is missing, ask for `pageId`.
   - If request is unrelated, ask for clarification.

2. Resolve authenticated GitHub identity:
   - Call `github/get_me`.
   - Extract `login`.
   - If `login` is missing, stop and report authentication issue.

3. Fetch all GitHub repositories (public + private):
   - Call `github/search_repositories` with query:
     - `user:{login} is:public`
   - Call `github/search_repositories` with query:
     - `user:{login} is:private`
   - For each repo, collect:
     - `full_name`
     - `private`
     - `html_url`
     - `default_branch`
     - `description` (if available)

4. Normalize repositories:
   - Deduplicate by `full_name`.
   - Keep `private = true/false` as source of truth.
   - Sort alphabetically by `full_name`.

5. Fetch Atlassian page details:
   - Call `atlassian/get_page` using provided `pageId`.
   - Collect page fields if available:
     - `id`
     - `title`
     - `status`
     - `space` (key/name)
     - `version`
     - `author`
     - `webUrl`
     - `body` summary/excerpt (if available)

6. Return structured response:
   - Include:
     - authenticated `login`
     - `publicRepoCount`
     - `privateRepoCount`
     - `totalRepoCount`
     - `repositories` array
     - `page` object
   - Preserve concise JSON-style output.

7. Error handling:
   - If `github/get_me` fails, report authentication/token scope issue.
   - If repo search fails, report which query failed (`public` or `private`).
   - If Atlassian page lookup fails, report invalid `pageId`/permissions/workspace access issue.
   - Do not invent repositories or page fields.

## Runtime Behavior

- Always call `github/get_me` first.
- Query GitHub repositories in two passes (public/private).
- Then call `atlassian/get_page` with provided `pageId`.
- Return machine-friendly JSON-style output only from tool results.

## Output Contract

Return:

- `login`
- `publicRepoCount`
- `privateRepoCount`
- `totalRepoCount`
- `repositories` (array of repo objects)
- `page` (Atlassian page details object)
- `errors` (array, only when failures occur)
