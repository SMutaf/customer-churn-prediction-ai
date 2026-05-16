import argparse
import sys

from .model_trainer import ModelTrainer


def main():
    parser = argparse.ArgumentParser(description="Train and export the CustomerAI churn model.")
    parser.parse_args()

    try:
        result = ModelTrainer().train()
    except Exception as error:
        print(f"Training failed: {error}")
        sys.exit(1)
    metrics = result["metrics"]
    metadata = result["metadata"]
    print("Training completed")
    print(f"Version: {metadata['version']}")
    print(f"Rows: {metadata['training_row_count']}")
    print(f"Accuracy: {metrics['accuracy']}")
    print(f"Precision: {metrics['precision']}")
    print(f"Recall: {metrics['recall']}")
    print(f"F1: {metrics['f1_score']}")


if __name__ == "__main__":
    main()
