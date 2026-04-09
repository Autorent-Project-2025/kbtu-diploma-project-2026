#!/bin/sh
set -eu

OLLAMA_BASE_URL="${OLLAMA_BASE_URL:-http://ollama:11434}"
CHAT_MODEL="${OLLAMA_CHAT_MODEL:-qwen2.5:1.5b}"
EMBEDDING_MODEL="${OLLAMA_EMBEDDING_MODEL:-nomic-embed-text}"

pull_model() {
  model="$1"
  echo "Pulling Ollama model: ${model}"
  curl -fsS "${OLLAMA_BASE_URL}/api/pull" \
    -H "Content-Type: application/json" \
    -d "{\"model\":\"${model}\",\"stream\":false}"
  echo
}

pull_model "${CHAT_MODEL}"
pull_model "${EMBEDDING_MODEL}"
