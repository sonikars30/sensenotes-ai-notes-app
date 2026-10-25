import re
from collections import Counter
from fastapi import FastAPI
from pydantic import BaseModel
from transformers import pipeline

app = FastAPI(title="SenseNotes ML service")
_models = {}

def model(name, task, model_id):
    """Load each Hugging Face pipeline once, on first use."""
    if name not in _models:
        _models[name] = pipeline(task, model=model_id)
    return _models[name]

STOP = set("the a an and or of to in is it that this for on with as are was be by at from".split())

def summarize(text, n=2):
    """Extractive summary: keep the n sentences whose words are most frequent in the note."""
    sentences = re.split(r"(?<=[.!?])\s+", text.strip())
    if len(sentences) <= n:
        return text
    freq = Counter(w for w in re.findall(r"[a-z']{3,}", text.lower()) if w not in STOP)
    scored = []
    for i, s in enumerate(sentences):
        words = re.findall(r"[a-z']{3,}", s.lower())
        scored.append((sum(freq[w] for w in words) / (len(s.split()) or 1), i, s))
    best = sorted(sorted(scored, reverse=True)[:n], key=lambda x: x[1])  # keep original order
    return " ".join(s for _, _, s in best)

class TextIn(BaseModel):
    text: str

@app.post("/analyze")
def analyze(body: TextIn):
    t = body.text.strip()
    sentiment = model("sent", "sentiment-analysis",
                      "distilbert-base-uncased-finetuned-sst-2-english")(t[:2000], truncation=True)[0]
    summary = summarize(t) if len(t.split()) > 60 else t  # only summarize longer notes
    words = [w for w in re.findall(r"[a-z']{3,}", t.lower()) if w not in STOP]
    return {"sentiment": {"label": sentiment["label"], "score": round(sentiment["score"], 3)},
            "summary": summary,
            "keywords": [w for w, _ in Counter(words).most_common(6)]}