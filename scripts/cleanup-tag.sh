#!/usr/bin/env bash
set -euo pipefail

TAG="${1:-}"
if [[ -z "$TAG" ]]; then
  echo "No tag provided; nothing to delete."
  exit 0
fi

echo "Cleaning up tag: $TAG"

gh release delete "$TAG" -y >/dev/null 2>&1 || echo "No release for $TAG or cannot delete."

gh api -X DELETE "repos/${GITHUB_REPOSITORY}/git/refs/tags/${TAG}" >/dev/null 2>&1 \
  && echo "Deleted tag ref tags/$TAG" \
  || echo "Tag tags/$TAG not found or cannot delete."