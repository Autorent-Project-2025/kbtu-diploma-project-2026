import uvicorn
from app import create_app
from app.settings import Settings

settings = Settings.from_env()
app = create_app(settings)


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=settings.port, reload=False)
