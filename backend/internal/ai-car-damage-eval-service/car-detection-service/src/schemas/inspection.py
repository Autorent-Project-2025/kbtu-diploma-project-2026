from datetime import datetime
from enum import Enum

from pydantic import BaseModel, Field


class Verdict(str, Enum):
    OK = "OK"
    DAMAGES_FOUND = "DAMAGES_FOUND"
    INVALID_SESSION = "INVALID_SESSION"


class PhotoSlot(str, Enum):
    """The five slots a booking-completion session expects.

    Callers upload one file per slot, labelled with one of these values.
    Every damage or rejected photo in the response carries the same slot
    value so the manager UI can render AI findings next to the exact
    photo that produced them without relying on fragile filename matching.
    """

    FRONT = "front"
    BACK = "back"
    SIDE_LEFT = "side_left"
    SIDE_RIGHT = "side_right"
    INTERIOR = "interior"


class Damage(BaseModel):
    type: str
    confidence: float = Field(ge=0, le=1)
    bbox: list[int] = Field(min_length=4, max_length=4)
    slot: PhotoSlot | None = None
    source_file: str | None = None


class RejectedPhoto(BaseModel):
    slot: PhotoSlot | None = None
    filename: str
    step: int
    reason: str
    details: list[str] = Field(default_factory=list)


class InspectionResult(BaseModel):
    verdict: Verdict
    damages: list[Damage]
    rejected_photos: list[RejectedPhoto]
    valid_photos_count: int
    processed_at_utc: datetime
