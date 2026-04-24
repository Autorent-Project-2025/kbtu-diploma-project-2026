# Fine-tuning AutoRent Assistant

Fine-tune qwen2.5:1.5b to understand car brands/models on Russian and English,
parse user queries into structured filters, and handle conversational requests.

## Requirements

- NVIDIA GPU with 6GB+ VRAM (RTX 3060 tested)
- CUDA 12.1+
- Python 3.10+
- Ollama (for deployment)

## Steps

### 1. Install dependencies

PyTorch must be installed first with CUDA support, then the rest:

```bash
pip install torch torchvision --index-url https://download.pytorch.org/whl/cu121
pip install "unsloth[cu121-torch250] @ git+https://github.com/unslothai/unsloth.git"
pip install -r requirements.txt
```

### 2. Generate training dataset

Make sure docker compose is running (need the ai-search DB):

```bash
# Find the ai-search-db port from docker-compose
python generate_dataset.py --db-url "postgresql://postgres:postgres@localhost:1836/postgres_db"
```

This creates `dataset.jsonl` with ~200+ training examples generated from:
- Car catalog (brand/model queries in RU and EN)
- Cyrillic aliases from `brand_model_aliases` table
- Style, budget, year, transmission queries
- Combined multi-filter queries
- Negative examples (greetings, gibberish)

### 3. Train

```bash
python train.py --dataset dataset.jsonl --output ./autorent-model --epochs 3
```

Takes ~10-15 minutes on RTX 3060. Uses:
- LoRA rank 16, 4-bit quantization
- ~4GB VRAM peak usage
- AdamW 8-bit optimizer

### 4. Export and load into Ollama

```bash
python export_gguf.py --model ./autorent-model
```

This exports to GGUF Q4_K_M and loads into Ollama as `autorent-assistant`.

### 5. Update docker-compose

```yaml
LOCAL_LLM_CHAT_MODEL: autorent-assistant
```

Restart ai-search-service. The fine-tuned model will be used for query parsing and response generation.

## Adding new cars

When new brands/models are added to the catalog:
1. Add aliases to `brand_model_aliases` table if needed
2. Re-run `generate_dataset.py` to include new cars
3. Re-run `train.py` (incremental training from previous checkpoint possible)
4. Re-export and reload into Ollama
