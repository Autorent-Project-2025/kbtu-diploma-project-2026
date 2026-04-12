"""
Export fine-tuned model to GGUF format and load into Ollama.

Usage:
    python export_gguf.py --model ./autorent-model
    # This will:
    # 1. Export to GGUF Q4_K_M quantization
    # 2. Create Ollama Modelfile
    # 3. Load into Ollama as 'autorent-assistant'
"""

import argparse
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", default="./autorent-model", help="Fine-tuned model directory")
    parser.add_argument("--output-gguf", default="./autorent-assistant.gguf", help="Output GGUF file")
    parser.add_argument("--quant", default="q4_k_m", help="Quantization type")
    parser.add_argument("--ollama-name", default="autorent-assistant", help="Ollama model name")
    args = parser.parse_args()

    print("Step 1: Exporting to GGUF via unsloth...")
    try:
        from unsloth import FastLanguageModel

        model, tokenizer = FastLanguageModel.from_pretrained(
            model_name=args.model,
            max_seq_length=2048,
            dtype=None,
            load_in_4bit=True,
        )

        model.save_pretrained_gguf(
            args.output_gguf.replace(".gguf", ""),
            tokenizer,
            quantization_method=args.quant,
        )
        print(f"GGUF exported: {args.output_gguf}")
    except Exception as e:
        print(f"Unsloth export failed: {e}")
        print("Alternative: use llama.cpp convert directly")
        sys.exit(1)

    print("Step 2: Creating Ollama Modelfile...")
    modelfile_content = f"""FROM {args.output_gguf}

TEMPLATE \"\"\"{{{{- if .System }}}}<|im_start|>system
{{{{ .System }}}}<|im_end|>
{{{{- end }}}}
<|im_start|>user
{{{{ .Prompt }}}}<|im_end|>
<|im_start|>assistant
\"\"\"

PARAMETER temperature 0
PARAMETER stop "<|im_end|>"
PARAMETER stop "<|im_start|>"
PARAMETER num_predict 256
"""

    modelfile_path = "Modelfile.autorent"
    with open(modelfile_path, "w") as f:
        f.write(modelfile_content)
    print(f"Modelfile written: {modelfile_path}")

    print(f"Step 3: Loading into Ollama as '{args.ollama_name}'...")
    result = subprocess.run(
        ["ollama", "create", args.ollama_name, "-f", modelfile_path],
        capture_output=True, text=True,
    )

    if result.returncode != 0:
        print(f"Ollama create failed: {result.stderr}")
        print(f"You can load manually: ollama create {args.ollama_name} -f {modelfile_path}")
        sys.exit(1)

    print(f"Model '{args.ollama_name}' loaded into Ollama!")
    print(f"\nTo use in docker-compose, set:")
    print(f"  LOCAL_LLM_CHAT_MODEL: {args.ollama_name}")


if __name__ == "__main__":
    main()
