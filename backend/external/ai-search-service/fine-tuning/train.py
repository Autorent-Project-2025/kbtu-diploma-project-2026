"""
Fine-tune qwen2.5:1.5b for AutoRent car recommendation parsing using LoRA.
Requires: pip install unsloth transformers datasets trl

Usage:
    python train.py --dataset dataset.jsonl --output ./autorent-model
    # Then export: python export_gguf.py --model ./autorent-model
"""

import argparse
import os
os.environ["HF_HUB_ENABLE_HF_TRANSFER"] = "0"

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--dataset", default="dataset.jsonl", help="Training JSONL file")
    parser.add_argument("--output", default="./autorent-model", help="Output model directory")
    parser.add_argument("--epochs", type=int, default=3, help="Training epochs")
    parser.add_argument("--batch-size", type=int, default=4, help="Per-device batch size")
    parser.add_argument("--lr", type=float, default=2e-4, help="Learning rate")
    parser.add_argument("--lora-r", type=int, default=16, help="LoRA rank")
    parser.add_argument("--max-seq-len", type=int, default=2048, help="Max sequence length")
    parser.add_argument("--base-model", default="unsloth/Qwen2.5-1.5B-Instruct",
                        help="Base model name or local path (e.g. ./qwen-base)")
    args = parser.parse_args()

    print("Loading unsloth...")
    from unsloth import FastLanguageModel
    from datasets import load_dataset
    from trl import SFTTrainer
    from transformers import TrainingArguments

    print(f"Loading base model: {args.base_model}...")
    model, tokenizer = FastLanguageModel.from_pretrained(
        model_name=args.base_model,
        max_seq_length=args.max_seq_len,
        dtype=None,  # auto-detect (float16 on 3060)
        load_in_4bit=True,  # saves VRAM
    )

    print("Applying LoRA adapters...")
    model = FastLanguageModel.get_peft_model(
        model,
        r=args.lora_r,
        target_modules=[
            "q_proj", "k_proj", "v_proj", "o_proj",
            "gate_proj", "up_proj", "down_proj",
        ],
        lora_alpha=args.lora_r,
        lora_dropout=0,
        bias="none",
        use_gradient_checkpointing="unsloth",
    )

    print(f"Loading dataset from {args.dataset}...")
    dataset = load_dataset("json", data_files=args.dataset, split="train")

    def format_chat(example):
        text = tokenizer.apply_chat_template(
            example["messages"],
            tokenize=False,
            add_generation_prompt=False,
        )
        return {"text": text}

    dataset = dataset.map(format_chat)
    print(f"Dataset: {len(dataset)} examples")

    trainer = SFTTrainer(
        model=model,
        tokenizer=tokenizer,
        train_dataset=dataset,
        dataset_text_field="text",
        max_seq_length=args.max_seq_len,
        dataset_num_proc=2,
        packing=False,
        args=TrainingArguments(
            per_device_train_batch_size=args.batch_size,
            gradient_accumulation_steps=4,
            warmup_steps=5,
            num_train_epochs=args.epochs,
            learning_rate=args.lr,
            fp16=False,
            bf16=True,
            logging_steps=5,
            optim="adamw_8bit",
            weight_decay=0.01,
            lr_scheduler_type="linear",
            seed=42,
            output_dir="./training-checkpoints",
        ),
    )

    print("Starting training...")
    stats = trainer.train()
    print(f"Training complete. Loss: {stats.training_loss:.4f}")

    print(f"Saving model to {args.output}...")
    model.save_pretrained(args.output)
    tokenizer.save_pretrained(args.output)
    print("Done! Now run: python export_gguf.py")


if __name__ == "__main__":
    main()
