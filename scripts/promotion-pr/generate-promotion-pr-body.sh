#!/usr/bin/env bash
set -euo pipefail

base_branch="${BASE_BRANCH:-staging}"
head_branch="${HEAD_BRANCH:-dev}"
pr_title_default="Promote: ${head_branch} → ${base_branch}"
pr_title="${PR_TITLE:-$pr_title_default}"
pr_labels="${PR_LABELS:-promotion,automation,env:${base_branch}}"
repo="${REPO:-${GITHUB_REPOSITORY:?REPO or GITHUB_REPOSITORY is required}}"
dry_run="${DRY_RUN:-false}"

tmp_body="$(mktemp)"
cleanup() {
  rm -f "$tmp_body"
}
trap cleanup EXIT

extract_release_line() {
  awk '
    BEGIN {found=0}
    /^## Release Notes/ {found=1; next}
    /^## / {if (found) exit}
    found && $0 ~ /[^[:space:]]/ {print; exit}
  '
}

extract_section_paragraph() {
  local heading="$1"
  awk -v h="$heading" '
    BEGIN {found=0; started=0}
    $0 ~ "^## " h {found=1; next}
    /^## / {if (found) exit}
    found {
      if ($0 ~ /[^[:space:]]/) {
        started=1
        print
      } else if (started) {
        exit
      }
    }
  '
}

categorize_pr() {
  local labels="$1"
  if [[ "$labels" == *"breaking"* ]]; then
    echo "Breaking changes"
  elif [[ "$labels" == *"feature"* ]]; then
    echo "Features"
  elif [[ "$labels" == *"bug"* ]]; then
    echo "Fixes"
  elif [[ "$labels" == *"tech-debt"* ]]; then
    echo "Tech debt"
  elif [[ "$labels" == *"infra"* ]]; then
    echo "Infrastructure"
  elif [[ "$labels" == *"docs"* ]]; then
    echo "Documentation"
  else
    echo "Other"
  fi
}

ensure_label() {
  local label="$1"
  local color="0E8A16"
  local description="Promotion label"

  case "$label" in
    promotion)
      description="Promotes changes between environment branches"
      color="0E8A16"
      ;;
    automation)
      description="Managed by automation"
      color="5319E7"
      ;;
    env:staging)
      description="Targets the staging environment"
      color="1D76DB"
      ;;
    env:prod)
      description="Targets the production environment"
      color="B60205"
      ;;
    release)
      description="Production release candidate"
      color="FBCA04"
      ;;
  esac

  if gh api "repos/${repo}/labels/${label}" >/dev/null 2>&1; then
    return 0
  fi

  if ! gh label create "$label" --repo "$repo" --color "$color" --description "$description" >/dev/null 2>&1; then
    echo "Warning: failed to create label '$label'." >&2
  fi
}

add_label_nonfatal() {
  local pr_number="$1"
  local label="$2"
  if ! gh pr edit "$pr_number" --add-label "$label" >/dev/null 2>&1; then
    echo "Warning: failed to add label '$label' to PR #$pr_number." >&2
  fi
}

IFS=',' read -r -a label_list <<< "$pr_labels"
for label in "${label_list[@]}"; do
  ensure_label "$label"
done

compare_json="$(gh api "repos/${repo}/compare/${base_branch}...${head_branch}")"
mapfile -t commit_shas < <(jq -r '.commits[].sha' <<<"$compare_json")
mapfile -t commit_messages < <(
  jq -r '.commits[] | "\(.sha[0:7]) \(.commit.message | split("\n")[0])"' <<<"$compare_json"
)

declare -A pr_numbers=()
for sha in "${commit_shas[@]}"; do
  while read -r pr_number; do
    if [[ -n "${pr_number}" ]]; then
      pr_numbers["${pr_number}"]=1
    fi
  done < <(
    gh api "repos/${repo}/commits/${sha}/pulls" \
      -H "Accept: application/vnd.github+json" \
      --jq '.[].number'
  )
done

sorted_prs=()
for pr in "${!pr_numbers[@]}"; do
  sorted_prs+=("${pr}")
done
IFS=$'\n' sorted_prs=($(sort -n <<<"${sorted_prs[*]}"))
unset IFS

declare -A grouped_lines=()
declare -a release_notes=()
declare -a migration_notes=()
declare -a test_notes=()

for pr in "${sorted_prs[@]:-}"; do
  pr_json="$(gh pr view "$pr" --json title,author,labels,url,body)"
  title="$(jq -r '.title' <<<"$pr_json")"
  url="$(jq -r '.url' <<<"$pr_json")"
  author="$(jq -r '.author.login' <<<"$pr_json")"
  labels="$(jq -r '.labels[].name' <<<"$pr_json" | paste -sd "," -)"
  body="$(jq -r '.body' <<<"$pr_json")"

  category="$(categorize_pr "$labels")"
  grouped_lines["$category"]+=$'- '"#${pr} ${title} (@${author}) - ${url}"$'\n'

  release_line="$(printf '%s\n' "$body" | extract_release_line)"
  if [[ -n "$release_line" ]]; then
    release_notes+=("- #${pr}: ${release_line}")
  else
    release_notes+=("- #${pr}: ${title}")
  fi

  migration_line="$(printf '%s\n' "$body" | extract_section_paragraph "Migration")"
  if [[ -z "$migration_line" ]]; then
    migration_line="$(printf '%s\n' "$body" | extract_section_paragraph "Database")"
  fi
  if [[ -z "$migration_line" ]]; then
    migration_line="$(printf '%s\n' "$body" | extract_section_paragraph "Deployment")"
  fi
  if [[ -n "$migration_line" ]]; then
    migration_notes+=("- #${pr}: ${migration_line}")
  fi

  test_line="$(printf '%s\n' "$body" | extract_section_paragraph "Test Evidence")"
  if [[ -n "$test_line" ]]; then
    test_notes+=("- #${pr}: ${test_line}")
  fi
done

overview_line="Promotion candidate from \`${head_branch}\` to \`${base_branch}\`."
if [[ ",${pr_labels}," == *",release,"* ]]; then
  overview_line="Release candidate from \`${head_branch}\` to \`${base_branch}\`."
fi

{
  echo "## Overview"
  if [[ ${#sorted_prs[@]} -eq 0 ]]; then
    echo "No changes pending promotion."
  else
    echo "$overview_line"
    if [[ ${#commit_messages[@]} -gt 0 ]]; then
      echo
      echo "Commits:"
      printf '%s\n' "${commit_messages[@]/#/- }"
    fi
  fi
  echo

  echo "## Included Pull Requests"
  if [[ ${#sorted_prs[@]} -eq 0 ]]; then
    echo "- No changes pending promotion."
  else
    for category in "Breaking changes" "Features" "Fixes" "Tech debt" "Infrastructure" "Documentation" "Other"; do
      if [[ -n "${grouped_lines[$category]:-}" ]]; then
        echo
        echo "### ${category}"
        printf '%s' "${grouped_lines[$category]}"
      fi
    done
  fi
  echo

  echo "## Notable Release Notes"
  if [[ ${#release_notes[@]} -eq 0 ]]; then
    echo "- No release notes provided."
  else
    printf '%s\n' "${release_notes[@]}"
  fi
  echo

  echo "## Migration / Deployment Notes"
  if [[ ${#migration_notes[@]} -eq 0 ]]; then
    echo "- None."
  else
    printf '%s\n' "${migration_notes[@]}"
  fi
  echo

  echo "## Test Notes"
  if [[ ${#test_notes[@]} -eq 0 ]]; then
    echo "- None."
  else
    printf '%s\n' "${test_notes[@]}"
  fi
  echo

  echo "## How to promote / rollback"
  echo "- Promote: merge this PR into \`${base_branch}\`."
  echo "- Rollback: revert the merge commit in \`${base_branch}\`."
} > "$tmp_body"

if [[ "$dry_run" == "true" ]]; then
  cat "$tmp_body"
  exit 0
fi

existing_pr_number="$(gh pr list --base "$base_branch" --head "$head_branch" --state open --json number --jq '.[0].number')"

if [[ -z "${existing_pr_number}" ]]; then
  echo "No open PR found for ${head_branch} -> ${base_branch}; create it manually."
  exit 0
fi

gh pr edit "$existing_pr_number" \
  --title "$pr_title" \
  --body-file "$tmp_body"

for label in "${label_list[@]}"; do
  add_label_nonfatal "$existing_pr_number" "$label"
done
