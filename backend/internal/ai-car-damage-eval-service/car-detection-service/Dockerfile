FROM python:3.11-slim

WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

# Создаем и используем непривилегированного пользователя для безопасности (Best Practice)
RUN useradd -m appuser && chown -R appuser /app
USER appuser

# Запуск через Gunicorn с использованием Uvicorn-воркеров
CMD ["gunicorn", "src.main:app", "--workers", "1", "--worker-class", "uvicorn.workers.UvicornWorker", "--bind", "0.0.0.0:8000"]
