from enum import Enum

from pydantic import BaseModel, Field


class Verdict(str, Enum):
    OK = "OK"
    DAMAGES_FOUND = "DAMAGES_FOUND"
    INVALID_SESSION = "INVALID_SESSION"


class Damage(BaseModel):
    type: str
    confidence: float = Field(ge=0, le=1)
    bbox: list[int] = Field(min_length=4, max_length=4)
    source_file: str | None = None


class RejectedPhoto(BaseModel):
    filename: str
    step: int
    reason: str
    details: list[str] = Field(default_factory=list)


class InspectionResult(BaseModel):
    verdict: Verdict
    damages: list[Damage]
    rejected_photos: list[RejectedPhoto]
    valid_photos_count: int
